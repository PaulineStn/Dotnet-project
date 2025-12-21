using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gauniv.Client.Models;
using Gauniv.Client.Repository;
using Gauniv.Client.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Gauniv.Client.ViewModel;

public partial class BuyViewModel : ObservableObject
{
	[ObservableProperty]
	private ObservableCollection<GameItemViewModel> gamesToBuy = new();

	[ObservableProperty]
	private bool isLoading;

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
	private async Task GameActionAsync(GameItemViewModel game)
	{
		if (game == null)
			return;

		if (game.Action == GameActionType.Buy)
		{
			await _userRepository.BuyGameAsync(game.Game.Id);
			await LoadGamesAsync();
		}
	}
}