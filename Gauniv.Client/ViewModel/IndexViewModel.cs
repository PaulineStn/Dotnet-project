using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Repository;
using Gauniv.Network;

namespace Gauniv.Client.ViewModel
{
    public partial class IndexViewModel: ObservableObject
    {

        [ObservableProperty]
        private ObservableCollection<GameDto> games = new();
        private readonly IGameRepository _repository;

        
        [ObservableProperty]
        private bool isLoading;

        public IndexViewModel(IGameRepository repository)
        {
            _repository = repository;
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
                {
                    Games.Add(local_game);
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

            // Navigation vers page détail plus tard
            Console.WriteLine($"Game sélectionné : {game.Name}");
        }
        
        
    }
}