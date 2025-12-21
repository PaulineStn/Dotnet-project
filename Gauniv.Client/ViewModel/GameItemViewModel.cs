using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Models;
using Gauniv.Client.Services;
using Gauniv.Network;

namespace Gauniv.Client.ViewModel;

public partial class GameItemViewModel : ObservableObject
{
    public GameDto Game { get; }

    public bool IsOwned { get; }
    public bool IsInstalled { get; }
    public bool IsLoggedIn { get; }
    public string? LocalVersion { get; }
    
    [ObservableProperty]
    private bool isGameRunning;

    [ObservableProperty]
    private GameActionType action;

    private readonly IGameInstallService _installService;

    public GameItemViewModel(
        GameDto game,
        bool isOwned,
        bool isLoggedIn,
        IGameInstallService installService)
    {
        Game = game;
        IsOwned = isOwned;
        IsLoggedIn = isLoggedIn;
        _installService = installService;

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

    public void SetPlayingState(bool running)
    {
        if (running)
        {
            Action = GameActionType.Playing;
            IsGameRunning = true;
        }
        else
        {
            Action = GameActionType.Play;
            IsGameRunning = false;
        }
    }
    
    [RelayCommand]
    private void StopGame(IGameInstallService installService)
    {
        _installService.StopGame();
    }

}