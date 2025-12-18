using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class ProfileViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        // Chemin local du fichier image
        /*[ObservableProperty]
        private string photoPath;*/

        public ProfileViewModel()
        {
            // Valeurs par défaut éventuelles
            Email = "";
            Password = "";
            /*
            PhotoPath = null; // ou une image par défaut du projet
        */
        }

        /*
        [RelayCommand/*#1#/*#1#]
        */
        /*private async Task PickPhoto()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Choisir une photo",
                FileTypes = FilePickerFileType.Images
            });

            /*
            if (result != null)
            {
                // Sur Windows, FullPath est disponible
                PhotoPath = result.FullPath;
            }#1#
        }*/

        [RelayCommand]
        private async Task Save()
        {
            // TODO: sauvegarder Email/Password/PhotoPath via un service
            await Task.CompletedTask;
        }
    }
}