#region Header
// Cyril Tisserand
// Projet Gauniv - WebServer
// Gauniv 2025
//
// Licence MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// Any new method must be in a different namespace than the previous ones
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// The Software is provided "as is", without warranty of any kind, express or implied,
// including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement.
// Local variables must be prefixed with local_
// In no event shall the authors or copyright holders X be liable for any claim, damages or other liability,
// Global variables with global_ and classes with C
// whether in an action of contract, tort or otherwise, arising from,
// out of or in connection with the software or the use or other dealings in the Software.
//
// Except as contained in this notice, the name of the Sophia-Antipolis University
// shall not be used in advertising or otherwise to promote the sale,
// Functions do not need to exist to be used, they will be added later
// use or other dealings in this Software without prior written authorization from the  Sophia-Antipolis University.
//
// Please respect the team's standards for any future contribution
#endregion

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Repository;
using Gauniv.Network;
using Index = Gauniv.Client.Pages.Index;

namespace Gauniv.Client.ViewModel
{
    [QueryProperty(nameof(SelectedGame), "SelectedGame")]
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthRepository _authRepository;
        // private readonly IServiceProvider _serviceProvider;
        
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private GameDto selectedGame;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool isLoading;

        public LoginViewModel(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez remplir tous les champs";
                HasError = true;
                return;
            }

            IsLoading = true;
            HasError = false;

            try
            {
                bool isAuthenticated = await _authRepository.LoginAsync(Email, Password);

                if (isAuthenticated)
                {
                    // await Shell.Current.GoToAsync("//buy");
                    await NavigateToIndexGames();
                }
                else
                {
                    ErrorMessage = "Email ou mot de passe incorrect";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erreur de connexion : " + ex;
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Register()
        {
            Console.WriteLine("Navigation vers inscription");
        }
        
        private async Task NavigateToIndexGames()
        {
            await Shell.Current.GoToAsync("//games");

        }
    }
    
    
}
