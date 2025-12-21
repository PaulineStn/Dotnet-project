using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Pages;

public partial class Register : ContentPage
{
    private readonly RegisterViewModel _viewModel;

    public Register(RegisterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
