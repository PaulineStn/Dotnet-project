using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Services;

using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;

public class GameInstallService : IGameInstallService
{
    private readonly string _installFolder;
    private readonly ConcurrentDictionary<int, string> _installedGames;

    public GameInstallService()
    {
        _installFolder = Path.Combine(FileSystem.AppDataDirectory, "Games");
       if(!Directory.Exists(_installFolder)) 
           Directory.CreateDirectory(_installFolder);

        _installedGames = new ConcurrentDictionary<int, string>();
        LoadInstalledGames();
    }

    private void LoadInstalledGames()
    {
        var jsonFile = Path.Combine(_installFolder, "installed.json");
        if (!File.Exists(jsonFile))
            return;

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

    public bool IsInstalled(int gameId)
    {
        return _installedGames.ContainsKey(gameId);
    }

    public string? GetLocalVersion(int gameId)
    {
        return _installedGames.TryGetValue(gameId, out var version) ? version : null;
    }

    public async Task DownloadAsync(int gameId)
    {
        // Simulation téléchargement
        await Task.Delay(1000); // simuler download
        _installedGames[gameId] = "1.0.0"; // version initiale
        SaveInstalledGames();
    }

    public async Task UpdateAsync(int gameId)
    {
        if (!_installedGames.ContainsKey(gameId))
            throw new InvalidOperationException("Jeu non installé");

        await Task.Delay(500); // simuler update
        _installedGames[gameId] = DateTime.UtcNow.Ticks.ToString(); // version simulée
        SaveInstalledGames();
    }

    public async Task PlayAsync(int gameId)
    {
        if (!_installedGames.ContainsKey(gameId))
            throw new InvalidOperationException("Jeu non installé");

        // Simulation lancement
        await Task.Delay(200);
        Console.WriteLine($"🎮 Lancement du jeu {gameId}...");
    }
}
