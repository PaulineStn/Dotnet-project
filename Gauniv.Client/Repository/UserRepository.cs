using Gauniv.Client.Services;
using Gauniv.Network;

namespace Gauniv.Client.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApiClient _api;
    private readonly IAuthService _authService;

    public UserRepository(ApiClient api, IAuthService authService)
    {
        _api = api;
        _authService = authService;
    }

    public async Task<IReadOnlyCollection<int>> GetMyPurchasedGameIdsAsync()
    {
        var token = await _authService.GetAccessTokenAsync();

        ApplyAuthorization(token);

        var result = await _api.GetMyPurchasesIdsAsync();

        return result.GameIds?.ToArray() ?? Array.Empty<int>();
    }

    public async Task BuyGameAsync(int gameId)
    {
        var token = await _authService.GetAccessTokenAsync();
        if (token == null)
            throw new InvalidOperationException("Utilisateur non connecté");

        ApplyAuthorization(token);

        await _api.PurchaseAsync(gameId);
    }

    private void ApplyAuthorization(string token)
    {
        _api.BearerToken = token;
    }
}