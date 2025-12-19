using Gauniv.Network;

namespace Gauniv.Client.Repository.Api;

public class ApiAuthRepository : IAuthRepository
{
    private readonly ApiClient _api;

    public ApiAuthRepository(ApiClient api)
    {
        _api = api;
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
            return response != null;
        }
        catch
        {
            return false;
        }
    }
}