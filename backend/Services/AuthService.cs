namespace SwedenStart;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IAuthRepo _repo;
    private readonly IConfiguration _config;

    public AuthService(IAuthRepo repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public async Task<string> LoginAsync(string email, string password)

    {
        email = email.Trim().ToLowerInvariant();
        var user = await _repo.GetUserByEmailAsync(email);
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        return await GenerateJwtToken(user);
    }

    public async Task<bool> RegisterAsync(string firstName, string lastName, string username, string email, string password)
    {

        firstName = firstName.Trim();
        lastName = lastName.Trim();
        username = username.Trim().ToLowerInvariant();
        email = email.Trim().ToLowerInvariant();

        if (await _repo.UserExistsAsync(username, email))
        {
            return false;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            IsActive = true,
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.CreateUserAsync(user);
        return true;
    }


    public Task<User?> GetUserAsync(Guid id)
    {
        return _repo.GetUserByIdAsync(id);
    }

    public Task<string> GenerateJwtToken(User user)
    {
        var key = _config["Jwt:Key"] ?? _config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is missing.");
        var issuer = _config["Jwt:Issuer"] ?? "sweden-start";
        var audience = _config["Jwt:Audience"] ?? "sweden-start-audience";
        var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) || int.TryParse(_config["Jwt:ExpiryMinutes"], out m) ? m : 60;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        };

        if (user.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.WriteToken(token);
        return Task.FromResult(jwt);
    }
}