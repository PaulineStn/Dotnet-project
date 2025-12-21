// File: `ViewModel/IndexViewModel.cs`
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Pages;
using Gauniv.Client.Repository;
using Gauniv.Network; // For CategoryDto
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
        private ObservableCollection<GameItemViewModel> filteredGames = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private ObservableCollection<string> categories = new();

        [ObservableProperty]
        private string? selectedCategory = null;

        [ObservableProperty]
        private double priceMinFilter;

        [ObservableProperty]
        private double priceMaxFilter;

        private double minPrice;
        private double maxPrice;

        public double MinPrice => minPrice;
        public double MaxPrice => maxPrice;

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

                // Extract all unique category names from all games
                var allCategories = local_games
                    .SelectMany(g => g.Categories ?? Enumerable.Empty<CategoryDto>())
                    .Select(c => c.Name)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                Categories = new ObservableCollection<string>(allCategories);
                SelectedCategory = null;

                minPrice = local_games.Any() ? (double)local_games.Min(g => g.Price) : 0;
                maxPrice = local_games.Any() ? (double)local_games.Max(g => g.Price) : 0;
                PriceMinFilter = minPrice;
                PriceMaxFilter = maxPrice;

                FilterGames();
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedCategoryChanged(string value)
        {
            FilterGames();
        }

        partial void OnPriceMinFilterChanged(double value)
        {
            FilterGames();
        }

        partial void OnPriceMaxFilterChanged(double value)
        {
            FilterGames();
        }

        private void FilterGames()
        {
            var filtered = Games.Where(g =>
                (string.IsNullOrEmpty(SelectedCategory) ||
                    (g.Game.Categories != null && g.Game.Categories.Any(c => c.Name == SelectedCategory))) &&
                (double)g.Game.Price >= PriceMinFilter &&
                (double)g.Game.Price <= PriceMaxFilter
            ).ToList();

            FilteredGames.Clear();
            foreach (var game in filtered)
                FilteredGames.Add(game);
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
            await Shell.Current.GoToAsync(nameof(Buy), navParam);
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