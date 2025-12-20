// File: `ViewModel/IndexViewModel.cs`
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Pages;
using Gauniv.Client.Repository;
using Gauniv.Network;
using System.Collections.ObjectModel;
using Gauniv.Client.Models;
using Gauniv.Client.Services;

namespace Gauniv.Client.ViewModel
{
    public partial class GameItem
    {
        public GameDto Game { get; }

        public bool IsOwned { get; }
        public bool IsInstalled { get; }
        public string? LocalVersion { get; }

        public GameActionType GameActionType
        {
            get { return _gameActionType; }
        }
        
        private GameActionType _gameActionType { get; set; }

        public GameItem(
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
                _gameActionType = GameActionType.Buy;
                return;
            }

            if (!IsInstalled)
            {
                _gameActionType = GameActionType.Download;
                return;
            }

            if (LocalVersion != Game.CurrentVersion)
            {
                _gameActionType = GameActionType.Update;
                return;
            }

            _gameActionType = GameActionType.Play;
        }
    }
    
    public partial class IndexViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<GameItem> games = new();

        private readonly IGameRepository _repository;
        
        [ObservableProperty]
        private bool isLoading;

        private readonly IGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IGameInstallService _installService;
        private readonly IServiceProvider _serviceProvider;
        
        public IndexViewModel(
            IGameRepository gameRepository,
            IUserRepository userRepository,
            IAuthService authService,
            IGameInstallService installService,
            IServiceProvider serviceProvider)
        {
            _gameRepository = gameRepository;
            _userRepository = userRepository;
            _authService = authService;
            _installService = installService;
            _serviceProvider = serviceProvider;
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
                    Games.Add(new GameItem(
                        local_game,
                        local_ownedGameIds.Contains(local_game.Id),
                        _installService
                        // local_isLoggedIn
                    ));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SelectGameCommand(GameDto game)
        {
            if (game == null)
                return;

            Console.WriteLine($"Game sélectionné : {game.Name}");
        }

        [RelayCommand]
        private async Task AcheterGameCommandAsync(GameDto game)
        {
            if (game == null)
                return;

            var local_loginPage = _serviceProvider.GetRequiredService<Login>();

            if (local_loginPage.BindingContext is LoginViewModel local_vm)
                local_vm.SelectedGame = game;

            await Application.Current.MainPage.Navigation.PushAsync(local_loginPage);
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
                    await _userRepository.BuyGameAsync(game.Game.Id);
                    await LoadGamesAsync();
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
                    await _installService.PlayAsync(game.Game.Id);
                    break;
            }
        }

        private async Task NavigateToLoginAsync(GameDto game)
        {
            var local_loginPage = _serviceProvider.GetRequiredService<Login>();

            if (local_loginPage.BindingContext is LoginViewModel local_vm)
                local_vm.SelectedGame = game;

            await Application.Current.MainPage.Navigation.PushAsync(local_loginPage);
        }
    }
}