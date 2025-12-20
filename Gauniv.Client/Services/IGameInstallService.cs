namespace Gauniv.Client.ViewModel;

public interface IGameInstallService
{
    /// <summary>
    /// Vérifie si le jeu est installé localement
    /// </summary>
    bool IsInstalled(int gameId);

    /// <summary>
    /// Récupère la version locale installée du jeu
    /// </summary>
    string? GetLocalVersion(int gameId);

    /// <summary>
    /// Télécharge le jeu localement
    /// </summary>
    // Task DownloadAsync(int gameId);

    /// <summary>
    /// Met à jour le jeu installé
    /// </summary>
    Task UpdateAsync(int gameId);

    /// <summary>
    /// Lance le jeu
    /// </summary>
    Task PlayAsync(int gameId);
}
