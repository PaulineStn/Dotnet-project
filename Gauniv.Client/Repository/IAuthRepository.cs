public interface IAuthRepository
{
    Task<bool> LoginAsync(string email, string password);

    Task<bool> RegisterAsync(string email, string password);
}