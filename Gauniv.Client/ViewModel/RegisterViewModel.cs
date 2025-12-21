using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Repository;
using System;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthRepository _authRepository;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private bool isLoading;

    public RegisterViewModel(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    [RelayCommand]
    private async Task CreateAccountAsync()
    {
        if (IsLoading)
            return;

        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Veuillez remplir tous les champs";
            HasError = true;
            return;
        }

        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            ErrorMessage = "Les mots de passe ne correspondent pas";
            HasError = true;
            return;
        }

        IsLoading = true;

        try
        {
            var local_ok = await _authRepository.RegisterAsync(Email, Password);
            if (!local_ok)
            {
                ErrorMessage = "Impossible de créer le compte";
                HasError = true;
                return;
            }

            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Erreur : " + ex.Message;
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BackToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }
}
