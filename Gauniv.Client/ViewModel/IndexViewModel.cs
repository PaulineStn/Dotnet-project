// File: `ViewModel/IndexViewModel.cs`
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Pages;
using Gauniv.Client.Repository;
using Gauniv.Network;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Gauniv.Client.Models;

namespace Gauniv.Client.ViewModel
{
    public partial class IndexViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<GameItemViewModel> games = new();

        [ObservableProperty]
        private bool isLoading;

        private readonly IGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IGameInstallService _installService;
        // private readonly IServiceProvider _serviceProvider;

        public IndexViewModel(
            IGameRepository gameRepository,
            IUserRepository userRepository,
            IAuthService authService,
            IGameInstallService installService)
        {
            _gameRepository = gameRepository;
            _userRepository = userRepository;
            _authService = authService;
            _installService = installService;
            // _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task LoadGamesAsync()
        {
            if (IsLoading)
                return;

            IsLoading = true;

            try
            {
                var local_games = await _gameRepository.GetAllAsync();
                var local_isLoggedIn = await _authService.IsLoggedInAsync();

                HashSet<int> local_ownedGameIds = [];

                if (local_isLoggedIn)
                {
                    var local_ids = await _userRepository.GetMyPurchasedGameIdsAsync();
                    local_ownedGameIds = local_ids.ToHashSet();
                }

                Games.Clear();

                foreach (var local_game in local_games)
                {
                    Games.Add(new GameItemViewModel(
                        local_game,
                        local_ownedGameIds.Contains(local_game.Id),
                        local_isLoggedIn,
                        _installService
                    ));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        // [RelayCommand]
        // private void SelectGameCommand(GameDto game)
        // {
        //     if (game == null)
        //         return;
        //
        //     Console.WriteLine($"Game sélectionné : {game.Name}");
        // }
        //
        // [RelayCommand]
        // private async Task BuyGame(GameDto game)
        // {
        //     if (game == null)
        //         return;
        //
        //     // Vérifier si l'utilisateur est connecté
        //     if (!_authService.IsAuthenticated)
        //     {
        //         var navigationParameter = new Dictionary<string, object>
        //         {
        //             { "SelectedGame", game }
        //         };
        //         await Shell.Current.GoToAsync("//login", navigationParameter);
        //     }
        //     else
        //     {
        //         // Rediriger directement vers Buy
        //         var navigationParameter = new Dictionary<string, object>
        //         {
        //             { "SelectedGame", game }
        //         };
        //         await Shell.Current.GoToAsync("//buy", navigationParameter);
        //     }
        // }
        
        [RelayCommand]
        private async Task NavigateToLoginAsync(GameDto selectedGame)
        {
            var navParam = new Dictionary<string, object>
            {
                { "SelectedGame", selectedGame }
            };
            await Shell.Current.GoToAsync("//login", navParam);
        }

        [RelayCommand]
        private async Task NavigateToBuyAsync(GameDto selectedGame)
        {
            var navParam = new Dictionary<string, object>
            {
                { "SelectedGame", selectedGame }
            };
            await Shell.Current.GoToAsync("//buy", navParam);
        }

        
        [RelayCommand]
        private async Task GameActionAsync(GameItemViewModel game)
        {
            if (game == null)
                return;

            switch (game.Action)
            {
                case GameActionType.LoginRequired:
                    await NavigateToLoginAsync(game.Game);
                    break;

                case GameActionType.Buy:
                    await NavigateToBuyAsync(game.Game);
                    break;

                case GameActionType.Download:
                    await _installService.DownloadAsync(game.Game.Id);
                    await LoadGamesAsync();
                    break;

                case GameActionType.Update:
                    await _installService.UpdateAsync(game.Game.Id);
                    await LoadGamesAsync();
                    break;

                case GameActionType.Play:
                    // await _installService.PlayAsync(game.Game.Id);
                    // break;
                    game.SetPlayingState(true);
                    await _installService.PlayAsync(game.Game.Id, () =>
                    {
                        // callback quand le jeu se termine
                        game.SetPlayingState(false);
                    });
                    break;
                
                case GameActionType.Playing:
                    // peut aussi arrêter via StopGame
                    _installService.StopGame();
                    game.SetPlayingState(false);
                    break;
            }
        }

    }
}