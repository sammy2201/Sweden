namespace SwedenStart;

public interface IAuthRepo
{
    Task<User?> GetUserByUsernameAsync(string username);

    Task<User?> GetUserByEmailAsync(string email);

    Task<User?> GetUserByIdAsync(Guid id);

    Task<bool> UserExistsAsync(string username, string email);

    Task CreateUserAsync(User user);

    Task UpdateUserAsync(User user);

    Task DeleteUserAsync(Guid id);
}