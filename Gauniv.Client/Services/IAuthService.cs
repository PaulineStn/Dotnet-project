public interface IAuthService
{
    bool IsAuthenticated { get; }
    string AccessToken { get; }
    void SetAuthentication(string token);
    void Logout();
}