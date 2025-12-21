using Gauniv.Client.ViewModel;

namespace Gauniv.Client.Pages;

public partial class Buy : ContentPage
{
	private readonly BuyViewModel _viewModel;

	public Buy(BuyViewModel buyViewModel)
	{
		InitializeComponent();
		_viewModel = buyViewModel;
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (_viewModel.LoadGamesCommand.CanExecute(null))
		{
			_viewModel.LoadGamesCommand.Execute(null);
		}
	}
}