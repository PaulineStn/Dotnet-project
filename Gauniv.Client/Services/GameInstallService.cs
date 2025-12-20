using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using Gauniv.Client.ViewModel;
using Gauniv.Network;

public class GameInstallService : IGameInstallService
{
    private readonly string _installFolder;
    private readonly ConcurrentDictionary<int, string> _installedGames;
    private readonly ApiClient _api;

    private Process? _runningGame;
    private CancellationTokenSource? _downloadCts;
    private Task? _gameTask;
    private CancellationTokenSource? _gameCts;
    private readonly IAuthService _authService;

    public GameInstallService(ApiClient api, IAuthService authService)
    {
        _api = api;
        _authService = authService;
        _installFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Games");

        if (!Directory.Exists(_installFolder))
            Directory.CreateDirectory(_installFolder);

        _installedGames = new ConcurrentDictionary<int, string>();
        LoadInstalledGames();
    }

    private void LoadInstalledGames()
    {
        var jsonFile = Path.Combine(_installFolder, "installed.json");
        if (!File.Exists(jsonFile)) return;

        var json = File.ReadAllText(jsonFile);
        var data = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
        if (data != null)
            foreach (var kv in data)
                _installedGames[kv.Key] = kv.Value;
    }

    private void SaveInstalledGames()
    {
        var jsonFile = Path.Combine(_installFolder, "installed.json");
        var json = JsonSerializer.Serialize(_installedGames);
        File.WriteAllText(jsonFile, json);
    }

    public bool IsInstalled(int gameId) => _installedGames.ContainsKey(gameId);

    public string? GetLocalVersion(int gameId) =>
        _installedGames.TryGetValue(gameId, out var version) ? version : null;
    
    /// <summary>
    /// Télécharge le jeu et renvoie le chemin complet
    /// </summary>
    public async Task<string> DownloadAsync(int gameId, IProgress<double>? progress = null)
    {
        var filename = GetFileName(gameId);
        if (IsInstalled(gameId))
            return Path.Combine(_installFolder, filename);

        _downloadCts = new CancellationTokenSource();
        
        // Ensure user is authenticated and get token
        var token = await _authService.GetAccessTokenAsync();
        if (token == null)
            throw new InvalidOperationException("User is not logged in.");
        _api.BearerToken = token;
        var game = await _api.GetAsync(gameId);

        // Get the HttpClient from NSwag-generated ApiClient
        var client = typeof(ApiClient)
            .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(_api) as HttpClient ?? throw new InvalidOperationException();

        
        
        // Set Authorization header
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var url = $"api/1.0.0/Games/Download/{gameId}";
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _downloadCts.Token);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1L;
        var filePath = Path.Combine(_installFolder, filename);

        await using var stream = await response.Content.ReadAsStreamAsync(_downloadCts.Token);
        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

        var buffer = new byte[81920];
        long totalRead = 0;
        int read;

        while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), _downloadCts.Token)) > 0)
        {
            await fs.WriteAsync(buffer.AsMemory(0, read), _downloadCts.Token);
            totalRead += read;
            if (total > 0)
                progress?.Report((double)totalRead / total * 100);
        }

        _installedGames[gameId] = game.CurrentVersion;
        SaveInstalledGames();

        return filePath;
    }

    public void CancelDownload()
    {
        _downloadCts?.Cancel();
    }

    public async Task UpdateAsync(int gameId)
    {
        if (!IsInstalled(gameId))
            throw new InvalidOperationException("Jeu non installé");

        await DownloadAsync(gameId);
        _installedGames[gameId] = DateTime.UtcNow.Ticks.ToString();
        SaveInstalledGames();
    }

    private string GetFileName(int gameId)
    {
        return $"{gameId}.exe";
        // return $"{gameId}.app";
    }

    public Task PlayAsync(int gameId, Action? onExited = null)
    {
        if (!IsInstalled(gameId))
            throw new InvalidOperationException("Jeu non installé");

        var filename = GetFileName(gameId);
        var exePath = Path.Combine(_installFolder, filename);
        if (!File.Exists(exePath))
            throw new FileNotFoundException("Fichier du jeu introuvable", exePath);

        _gameCts = new CancellationTokenSource();
        var token = _gameCts.Token;

        _gameTask = Task.Run(() =>
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            };
            // {
            //     FileName = "open";
            //     Arguments = $"-a \"{exePath}\""; // -a pour application
            //     UseShellExecute = false;
            // };
            Console.WriteLine($"exePath:  {exePath}");
           

            _runningGame = Process.Start(startInfo);

            Console.WriteLine($"🎮 Jeu {gameId} lancé (PID {_runningGame?.Id})");

            try
            {
                while (!_gameCts.Token.IsCancellationRequested && !_runningGame.HasExited)
                {
                    Task.Delay(500, _gameCts.Token).Wait();
                }

                if (!_runningGame.HasExited)
                    _runningGame.Kill();
            }
            catch (OperationCanceledException)
            {
                if (_runningGame != null && !_runningGame.HasExited)
                    _runningGame.Kill();
            }
            finally
            {
                _runningGame = null;
                _gameTask = null;
                _gameCts.Dispose();
                _gameCts = null;
                onExited?.Invoke();
                Console.WriteLine("🛑 Jeu terminé");
            }
        }, _gameCts.Token);

        return _gameTask;
    }

    public void StopGame()
    {
        if (_gameCts == null) return;

        try
        {
            if (!_gameCts.IsCancellationRequested)
                _gameCts.Cancel();

            if (_runningGame != null && !_runningGame.HasExited)
            {
                _runningGame.Kill();
                Console.WriteLine($"🛑 Jeu arrêté (PID {_runningGame.Id})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'arrêt du jeu : {ex.Message}");
        }
        finally
        {
            _runningGame = null;
            _gameTask = null;
            _gameCts.Dispose();
            _gameCts = null;
        }
    }

    
}
