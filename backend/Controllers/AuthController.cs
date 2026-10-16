using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        return Ok(new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
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
            return Conflict(ApiProblemDetails.Create(
                StatusCodes.Status409Conflict,
                "Account already exists",
                "A user with that username or email already exists.",
                "ACCOUNT_ALREADY_EXISTS"));
        }

        return CreatedAtAction(nameof(Register), new { username = request.Username });
    }


    [HttpGet("user")]
    [Authorize]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser()
    {
        if (!TryGetCurrentUserId(out var id))
            return Unauthorized(ApiProblemDetails.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified."));

        var user = await _authService.GetUserAsync(id);
        if (user == null)
            return NotFound(ApiProblemDetails.Create(StatusCodes.Status404NotFound, "User not found", "The requested user profile was not found."));

        var dto = MapUserDto(user);

        return Ok(dto);
    }

    [HttpPut("user")]
    [Authorize]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserProfileRequestDto request)
    {
        if (!TryGetCurrentUserId(out var id))
            return Unauthorized(ApiProblemDetails.Create(StatusCodes.Status401Unauthorized, "Unauthorized", "The authenticated user could not be identified."));

        var user = await _authService.UpdateUserProfileAsync(
            id,
            request.FirstName,
            request.LastName,
            request.City,
            request.Address);

        if (user == null)
            return NotFound(ApiProblemDetails.Create(StatusCodes.Status404NotFound, "User not found", "The requested user profile was not found."));

        return Ok(MapUserDto(user));
    }

    private bool TryGetCurrentUserId(out Guid id)
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out id);
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
            Address = user.Address
        };
    }
}
