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
    public AuthController(IAuthService authService)
    {
        _authService = authService;

    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        var expiresAt = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        return Ok(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var created = await _authService.RegisterAsync(request.FirstName, request.LastName, request.Username, request.Email, request.City, request.Address, request.Password);
        if (!created)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Account already exists",
                "A user with that username or email already exists.",
                "ACCOUNT_ALREADY_EXISTS");
        }

        return CreatedAtAction(nameof(Register), new { username = request.Username });
    }


    [HttpGet("user")]
    [Authorize]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser()
    {
        if (!TryGetCurrentUserId(out var userId))
            throw new ApiException(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified.");

        var user = await _authService.GetUserAsync(userId);
        if (user == null)
            throw new ApiException(StatusCodes.Status404NotFound, "User not found", "The requested user profile was not found.");

        var userDto = MapUserDto(user);

        return Ok(userDto);
    }

    [HttpPut("user")]
    [Authorize]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserProfileRequestDto request)
    {
        if (request.TransportCountyId is not null && !CountyData.IsValidCountyId(request.TransportCountyId))
            throw new ApiException(StatusCodes.Status400BadRequest, "Invalid transport region", "Select a valid Swedish county for public transport links.");

        if (!TryGetCurrentUserId(out var userId))
            throw new ApiException(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified.");

        var user = await _authService.UpdateUserProfileAsync(
            userId,
            request.FirstName,
            request.LastName,
            request.City,
            request.Address,
            request.TransportCountyId);

        if (user == null)
            throw new ApiException(StatusCodes.Status404NotFound, "User not found", "The requested user profile was not found.");

        return Ok(MapUserDto(user));
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdentifierClaim, out userId);
    }

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
            Address = user.Address,
            TransportCountyId = user.TransportCountyId
        };
    }
}
