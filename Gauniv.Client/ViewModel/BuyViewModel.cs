using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Models;
using Gauniv.Client.Repository;
using Gauniv.Client.Services;
using Gauniv.Network;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel;

[QueryProperty(nameof(SelectedGame), "SelectedGame")]
public partial class BuyViewModel : ObservableObject
{
	[ObservableProperty]
	private ObservableCollection<GameItemViewModel> gamesToBuy = new();

	[ObservableProperty]
	private bool isLoading;

	[ObservableProperty]
	private GameDto? selectedGame;

	private readonly IGameRepository _gameRepository;
	private readonly IUserRepository _userRepository;
	private readonly IAuthService _authService;
	private readonly IGameInstallService _installService;

	public BuyViewModel(
		IGameRepository gameRepository,
		IUserRepository userRepository,
		IAuthService authService,
		IGameInstallService installService)
	{
		_gameRepository = gameRepository;
		_userRepository = userRepository;
		_authService = authService;
		_installService = installService;
	}

	public bool HasSelectedGame => SelectedGame != null;
	public bool HasNoSelectedGame => SelectedGame == null;

	partial void OnSelectedGameChanged(GameDto? value)
	{
		OnPropertyChanged(nameof(HasSelectedGame));
		OnPropertyChanged(nameof(HasNoSelectedGame));
	}

	[RelayCommand]
	private async Task LoadGamesAsync()
	{
		if (IsLoading)
			return;

		IsLoading = true;

		try
		{
			var local_games = await _gameRepository.GetAllAsync();
			var local_isLoggedIn = await _authService.IsLoggedInAsync();

			HashSet<int> local_ownedGameIds = [];

			if (local_isLoggedIn)
			{
				var local_ids = await _userRepository.GetMyPurchasedGameIdsAsync();
				local_ownedGameIds = local_ids.ToHashSet();
			}

			GamesToBuy.Clear();

			foreach (var local_game in local_games)
			{
				var local_item = new GameItemViewModel(
					local_game,
					local_ownedGameIds.Contains(local_game.Id),
					local_isLoggedIn,
					_installService);

				if (local_item.Action == GameActionType.Buy)
				{
					GamesToBuy.Add(local_item);
				}
			}
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	private void SelectForConfirmation(GameItemViewModel game)
	{
		if (game == null)
			return;

		SelectedGame = game.Game;
	}

	[RelayCommand]
	private async Task ConfirmPurchaseAsync()
	{
		if (SelectedGame == null)
			return;

		if (IsLoading)
			return;

		IsLoading = true;
		try
		{
			var local_isLoggedIn = await _authService.IsLoggedInAsync();
			if (!local_isLoggedIn)
			{
				await Shell.Current.DisplayAlertAsync("Login required", "Please login to purchase games.", "OK");
				await Shell.Current.GoToAsync("//login", new Dictionary<string, object>
				{
					{ "SelectedGame", SelectedGame }
				});
				return;
			}

			try
			{
				await _userRepository.BuyGameAsync(SelectedGame.Id);
			}
			catch (Exception ex)
			{
				// Detect HTTP 401 Unauthorized
				if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
				{
					await Shell.Current.DisplayAlertAsync("Login required", "Your session has expired. Please login again to purchase games.", "OK");
					await Shell.Current.GoToAsync("//login", new Dictionary<string, object>
					{
						{ "SelectedGame", SelectedGame }
					});
					return;
				}
				await Shell.Current.DisplayAlertAsync("Purchase failed", ex.Message, "OK");
				return;
			}

			SelectedGame = null;
		}
		finally
		{
			IsLoading = false;
		}

		await LoadGamesAsync();
		await Shell.Current.GoToAsync("//games");
	}

	[RelayCommand]
	private async Task GameActionAsync(GameItemViewModel game)
	{
		if (game == null)
			return;

		if (game.Action == GameActionType.Buy)
		{
			SelectedGame = game.Game;
		}
	}
}