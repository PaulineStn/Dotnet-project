using Gauniv.Client.ViewModel;
using Gauniv.Network;

namespace Gauniv.Client.Pages;

public partial class Login : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public Login(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}