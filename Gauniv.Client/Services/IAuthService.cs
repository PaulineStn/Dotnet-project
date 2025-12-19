public interface IAuthService
{
    bool IsAuthenticated { get; }
    string AccessToken { get; }
    void SetAuthentication(string token);
    void Logout();
}

public class AuthService : IAuthService
{
    private string _accessToken;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);
    public string AccessToken => _accessToken;

    public void SetAuthentication(string token)
    {
        _accessToken = token;
    }

    public void Logout()
    {
        _accessToken = null;
    }
}