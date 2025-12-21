using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Gauniv.Client.Models;
using Gauniv.Client.Pages;
using Gauniv.Client.Repository;
using Gauniv.Client.Services;
using Gauniv.Network;

namespace Gauniv.Client.ViewModel
{
    public partial class MyGamesViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<GameItemViewModel> gamesToBuy = new();

        [ObservableProperty]
        private bool isLoading;

        private readonly IGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IGameInstallService _installService;

        public MyGamesViewModel(
            IGameRepository gameRepository,
            IUserRepository userRepository,
            IAuthService authService,
            IGameInstallService installService)
        {
            _gameRepository = gameRepository;
            _userRepository = userRepository;
            _authService = authService;
            _installService = installService;
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

                GamesToBuy.Clear();

                foreach (var local_game in local_games)
                {
                    var local_item = new GameItemViewModel(
                        local_game,
                        local_ownedGameIds.Contains(local_game.Id),
                        local_isLoggedIn,
                        _installService);

                    // Afficher tous les jeux achetés, peu importe l'action
                    if (local_item.IsOwned)
                    {
                        GamesToBuy.Add(local_item);
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GameActionAsync(GameItemViewModel game)
        {
            if (game == null)
                return;

            switch (game.Action)
            {
                case GameActionType.LoginRequired:
                    await Shell.Current.GoToAsync("//login");
                    break;

                case GameActionType.Buy:
                    await Shell.Current.GoToAsync(nameof(Buy), new Dictionary<string, object>
                    {
                        { "SelectedGame", game.Game }
                    });
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
                    game.SetPlayingState(true);
                    await _installService.PlayAsync(game.Game.Id, () =>
                    {
                        game.SetPlayingState(false);
                    });
                    break;

                case GameActionType.Playing:
                    _installService.StopGame();
                    game.SetPlayingState(false);
                    break;
            }
        }
    }
}
