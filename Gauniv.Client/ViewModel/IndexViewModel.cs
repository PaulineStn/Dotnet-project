using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Gauniv.Network;

namespace Gauniv.Client.ViewModel
{
    public partial class IndexViewModel: ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<GameDto> games;

        public IndexViewModel()
        {
            LoadGames();
        }

        private void LoadGames()
        {
            
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5231")
            };
            
            var api = new ApiClient(httpClient);
                
            var games = api.GetAllAsync(
                categoryId: null,
                minPrice: null,
                maxPrice: null,
                search: null
            );
            
            Console.WriteLine($"games: {games.Result}");

            Games = new ObservableCollection<GameDto>(games.Result);
            // Games = new ObservableCollection<Game>
            // {
            //     new Game { Title = "Jeu 1", ImagePath = "jeu1.jpg" ,Description="lorem ipsum"},
            //     new Game { Title = "Jeu 2", ImagePath = "jeu2.jpg",Description="lorem ipsum" },
            // };
        }

        [RelayCommand]
        private async Task SelectGame(Game game)
        {
            // Navigation vers les détails du jeu
            await Shell.Current.GoToAsync($"gamedetails?id={game.Id}");
        }
    }
}
