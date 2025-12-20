using Gauniv.Network;

namespace Gauniv.Client.Repository.Api;

public class ApiAuthRepository : IAuthRepository
{
    private readonly ApiClient _api;
    private readonly IAuthService _authService;

    public ApiAuthRepository(ApiClient api, IAuthService authService)
    {
        _api = api;
        _authService = authService;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _api.LoginAsync(null, null, request);
            _authService.SetAuthentication(response.AccessToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}