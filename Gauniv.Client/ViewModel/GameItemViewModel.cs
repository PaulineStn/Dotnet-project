using CommunityToolkit.Mvvm.ComponentModel;
using Gauniv.Client.Models;
using Gauniv.Client.ViewModel;
using Gauniv.Network;

public partial class GameItemViewModel : ObservableObject
{
    public GameDto Game { get; }

    public bool IsOwned { get; }
    public bool IsInstalled { get; }
    public bool IsLoggedIn { get; }
    public string? LocalVersion { get; }

    [ObservableProperty]
    private GameActionType action;

    public GameItemViewModel(
        GameDto game,
        bool isOwned,
        bool isLoggedIn,
        IGameInstallService installService)
    {
        Game = game;
        IsOwned = isOwned;
        IsLoggedIn = isLoggedIn;

        IsInstalled = installService.IsInstalled(game.Id);
        LocalVersion = installService.GetLocalVersion(game.Id);

        ComputeAction();
    }

    private void ComputeAction()
    {

        if (!IsLoggedIn)
        {
            Action =  GameActionType.LoginRequired;
            return;
        }
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
    
    public string ActionLabel => Action switch
    {
        GameActionType.LoginRequired => "Login",
        GameActionType.Buy => "Buy",
        GameActionType.Download => "Download",
        GameActionType.Update => "Update",
        GameActionType.Play => "Play",
        _ => ""
    };
}