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
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private bool isLoading;

        public IndexViewModel(IGameRepository repository, IServiceProvider serviceProvider)
        {
            _repository = repository;
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
        private async Task AcheterGame(GameDto game)
        {
            if (game == null)
                return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "SelectedGame", game }
            };

            Debug.WriteLine($"Achat du jeu : {game.Name}");
            await Shell.Current.GoToAsync("//login",navigationParameter);
        }

    }
}