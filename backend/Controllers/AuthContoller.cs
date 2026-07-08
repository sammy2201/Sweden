using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SwedenStart;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var token = await _authService.LoginAsync(request.Email, request.Password);
            return Ok(new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _authService.RegisterAsync(request.FirstName, request.LastName, request.Username, request.Email, request.Password);
        if (!created) return Conflict(new { message = "User with that username or email already exists" });

        return CreatedAtAction(nameof(Register), new { username = request.Username });
    }


    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUser()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub)) return Unauthorized();

        if (!Guid.TryParse(sub, out var id)) return Unauthorized();

        var user = await _authService.GetUserAsync(id);
        if (user == null) return NotFound();

        var dto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Email = user.Email
        };

        return Ok(dto);
    }
}