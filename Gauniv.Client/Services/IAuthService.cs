public interface IAuthService
{
    bool IsAuthenticated { get; }
    string AccessToken { get; }
    void SetAuthentication(string token, long expiresIn);
    void Logout();
    public Task LogoutAsync();
    public Task<string> GetAccessTokenAsync();
    public Task<bool> IsLoggedInAsync();
}