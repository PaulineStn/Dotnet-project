public interface IAuthService
{
    bool IsAuthenticated { get; }
    string AccessToken { get; }
    void SetAuthentication(string token);
    void Logout();

    public Task<string> GetAccessTokenAsync();
    public Task<bool> IsLoggedInAsync();
}