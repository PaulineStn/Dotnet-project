using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Pages;
using Gauniv.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel
{
    public partial class MenuViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private bool isUserConnected;
        
        [RelayCommand]
        public void GoToProfile() => NavigationService.Instance.Navigate<Profile>([]);

        [ObservableProperty]
        private bool isConnected = NetworkService.Instance.Token != null;

        public MenuViewModel(IAuthService authService)
        {
            NetworkService.Instance.OnConnected += Instance_OnConnected;
            _authService = authService;
            IsUserConnected = _authService.IsLoggedInAsync().Result;
        }

        private void Instance_OnConnected()
        {
            IsConnected = true;
        }
        
        [RelayCommand]
        public async Task LogoutAsync()
        {
            if (!await _authService.IsLoggedInAsync())
                return;
            await _authService.LogoutAsync();
            IsUserConnected = false;

            // Rediriger vers la page login
            await Shell.Current.GoToAsync("//login");
        }
    }
}
