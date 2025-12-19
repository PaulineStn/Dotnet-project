public interface IAuthRepository
{
    Task<bool> LoginAsync(string email, string password);
}