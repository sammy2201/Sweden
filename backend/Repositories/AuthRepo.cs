using Microsoft.EntityFrameworkCore;

namespace SwedenStart;

public class AuthRepo : IAuthRepo
{
    private readonly AppDbContext _dbContext;


    public AuthRepo(AppDbContext dbContext)
    {
        _dbContext = dbContext;

    }

    public Task<User?> GetUserByUsernameRepoAsync(string username)
        => _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == username);

    public Task<User?> GetUserByEmailRepoAsync(string email)
        => _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email);

    public Task<User?> GetUserByIdRepoAsync(Guid id)
        => _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id);

    public Task<bool> UserExistsRepoAsync(string username, string email)
        => _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Username == username || user.Email == email);

    public async Task CreateUserRepoAsync(User user)
    {
        user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
        user.CreatedAt = user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt;

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateUserRepoAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserRepoAsync(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
    }


}
