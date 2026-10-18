
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SwedenStart;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // Constructor initializes the controller's private field with the auth service.
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var token = await _authService.LoginAsync(
            request.Email,
            request.Password);

        var expiresAt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token)
            .ValidTo;

        return Ok(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request)
    {
        var created = await _authService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Username,
            request.Email,
            request.City,
            request.Address,
            request.Password);

        if (!created)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Account already exists",
                "A user with that username or email already exists.",
                "ACCOUNT_ALREADY_EXISTS");
        }

        return StatusCode(
            StatusCodes.Status201Created,
            "Registration successful");
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUser()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            throw new ApiException(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated user could not be identified.");
        }

        var user = await _authService.GetUserAsync(userId.Value);

        if (user == null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "User not found",
                "The requested user profile was not found.");
        }

        return Ok(MapUserDto(user));
    }

    [HttpPut("user")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(
        [FromBody] UpdateUserProfileRequestDto request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            throw new ApiException(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "The authenticated user could not be identified.");
        }

        var user = await _authService.UpdateUserProfileAsync(
            userId.Value,
            request.FirstName,
            request.LastName,
            request.City,
            request.Address);

        if (user == null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "User not found",
                "The requested user profile was not found.");
        }

        return Ok(MapUserDto(user));
    }

    // Gets the current user's ID from the authentication claims.
    private Guid? GetCurrentUserId()
    {
        var userIdentifierClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdentifierClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    // Converts the database user into a DTO for the API response.
    private static UserDto MapUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Email = user.Email,
            City = user.City,
            Address = user.Address
        };
    }
}
