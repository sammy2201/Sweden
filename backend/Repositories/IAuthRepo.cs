namespace SwedenStart;

public interface IAuthRepo
{
    Task<User?> GetUserByUsernameRepoAsync(string username);

    Task<User?> GetUserByEmailRepoAsync(string email);

    Task<User?> GetUserByIdRepoAsync(Guid id);

    Task<bool> UserExistsRepoAsync(string username, string email);

    Task CreateUserRepoAsync(User user);

    Task UpdateUserRepoAsync(User user);

    Task DeleteUserRepoAsync(Guid id);
}