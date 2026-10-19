namespace SwedenStart;

public interface IAuthService
{
    Task<string> LoginServiceAsync(string email, string password);
    Task<bool> RegisterServiceAsync(string firstName, string lastName, string username, string email, string city, string address, string password);
    Task<User?> GetUserServiceAsync(Guid id);
    Task<User?> UpdateUserProfileServiceAsync(Guid id, string firstName, string lastName, string city, string address);
}
