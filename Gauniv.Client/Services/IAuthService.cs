using Gauniv.Network;

namespace Gauniv.Client.Services;

public interface IAuthService
{
    Task<bool> IsLoggedInAsync();
    Task<AccessTokenResponse?> GetAccessTokenAsync();
    Task LoginAsync(string username, string password);
    Task LogoutAsync();
    Task RefreshTokenIfNeededAsync();
}