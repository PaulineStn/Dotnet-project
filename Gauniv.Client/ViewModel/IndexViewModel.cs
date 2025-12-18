using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class IndexViewModel: ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Game> games;

        public IndexViewModel()
        {
            LoadGames();
        }

        private void LoadGames()
        {
            Games = new ObservableCollection<Game>
            {
                new Game { Title = "Jeu 1", ImagePath = "jeu1.jpg" ,Description="lorem ipsum"},
                new Game { Title = "Jeu 2", ImagePath = "jeu2.jpg",Description="lorem ipsum" },
            };
        }

        [RelayCommand]
        private async Task SelectGame(Game game)
        {
            // Navigation vers les détails du jeu
            await Shell.Current.GoToAsync($"gamedetails?id={game.Id}");
        }
    }
}
