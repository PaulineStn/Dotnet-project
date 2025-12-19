// File: `ViewModel/IndexViewModel.cs`
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Pages;
using Gauniv.Client.Repository;
using Gauniv.Network;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Gauniv.Client.ViewModel
{
    public partial class IndexViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<GameDto> games = new();

        private readonly IGameRepository _repository;
        private readonly IAuthService _authService;
        [ObservableProperty]
        private bool isLoading;

        public IndexViewModel(IGameRepository repository, IAuthService authService)
        {
            _repository = repository;
            _authService = authService;

        }

        [RelayCommand]
        private async Task LoadGamesAsync()
        {
            if (IsLoading)
                return;

            IsLoading = true;

            try
            {
                var local_games = await _repository.GetAllAsync();
                Games.Clear();

                foreach (var local_game in local_games)
                    Games.Add(local_game);
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
        private async Task BuyGame(GameDto game)
        {
            if (game == null)
                return;

            // Vérifier si l'utilisateur est connecté
            if (!_authService.IsAuthenticated)
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "SelectedGame", game }
                };
                await Shell.Current.GoToAsync("//login", navigationParameter);
            }
            else
            {
                // Rediriger directement vers Buy
                var navigationParameter = new Dictionary<string, object>
                {
                    { "SelectedGame", game }
                };
                await Shell.Current.GoToAsync("//buy", navigationParameter);
            }
        }

    }
}