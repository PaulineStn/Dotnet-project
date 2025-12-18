using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Gauniv.Network;

namespace Gauniv.Client.ViewModel
{
    public partial class IndexViewModel: ObservableObject
    {
        private readonly ApiClient _api;

        [ObservableProperty]
        private ObservableCollection<GameDto> games = new();

        public IndexViewModel(ApiClient api)
        {
            _api = api;
            LoadGames();
        }
        
        private void LoadGames()
        {
                
            var req = _api.GetAllAsync(null, null, null, null, null, null);
            
            Console.WriteLine($"games: {req.Result}");
        
            Games = new ObservableCollection<GameDto>(req.Result);
            // Games = new ObservableCollection<Game>
            // {
            //     new Game { Title = "Jeu 1", ImagePath = "jeu1.jpg" ,Description="lorem ipsum"},
            //     new Game { Title = "Jeu 2", ImagePath = "jeu2.jpg",Description="lorem ipsum" },
            // };
        }

        // [RelayCommand]
        // private async Task SelectGame(Game game)
        // {
        //     // Navigation vers les détails du jeu
        //     await Shell.Current.GoToAsync($"gamedetails?id={game.Id}");
        // }
    }
}