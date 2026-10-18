using Microsoft.EntityFrameworkCore;

namespace SwedenStart;

public class AuthRepo : IAuthRepo
{
    private readonly AppDbContext _dbContext;


    public AuthRepo(AppDbContext dbContext)
    {
        _dbContext = dbContext;

    }

    public Task<User?> GetUserByUsernameAsync(string username)
        => _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == username);

    public Task<User?> GetUserByEmailAsync(string email)
        => _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email);

    public Task<User?> GetUserByIdAsync(Guid id)
        => _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id);

    public Task<bool> UserExistsAsync(string username, string email)
        => _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Username == username || user.Email == email);

    public async Task CreateUserAsync(User user)
    {
        user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
        user.CreatedAt = user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt;

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
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
