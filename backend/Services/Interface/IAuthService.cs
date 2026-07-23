namespace SwedenStart;

public interface IAuthService
{
    Task<string> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string firstName, string lastName, string username, string email, string city, string address, string password);
    Task<string> GenerateJwtToken(User user);

    Task<User?> GetUserAsync(Guid id);
}