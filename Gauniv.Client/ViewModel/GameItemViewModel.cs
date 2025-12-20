using CommunityToolkit.Mvvm.ComponentModel;
using Gauniv.Client.Models;
using Gauniv.Client.ViewModel;
using Gauniv.Network;

public partial class GameItemViewModel : ObservableObject
{
    public GameDto Game { get; }

    public bool IsOwned { get; }
    public bool IsInstalled { get; }
    public string? LocalVersion { get; }

    [ObservableProperty]
    private GameActionType action;

    public GameItemViewModel(
        GameDto game,
        bool isOwned,
        IGameInstallService installService)
    {
        Game = game;
        IsOwned = isOwned;

        IsInstalled = installService.IsInstalled(game.Id);
        LocalVersion = installService.GetLocalVersion(game.Id);

        ComputeAction();
    }

    private void ComputeAction()
    {
        if (!IsOwned)
        {
            Action = GameActionType.Buy;
            return;
        }

        if (!IsInstalled)
        {
            Action = GameActionType.Download;
            return;
        }

        if (LocalVersion != Game.CurrentVersion)
        {
            Action = GameActionType.Update;
            return;
        }

        Action = GameActionType.Play;
    }
}