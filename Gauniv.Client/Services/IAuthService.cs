public interface IAuthService
{
    bool IsAuthenticated { get; }
    string AccessToken { get; }
    void SetAuthentication(string token);
    void Logout();
    public Task LogoutAsync();
    public Task<string> GetAccessTokenAsync();
    public Task<bool> IsLoggedInAsync();
}