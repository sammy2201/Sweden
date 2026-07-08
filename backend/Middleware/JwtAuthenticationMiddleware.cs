using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SwedenStart;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenValidationParameters _validationParameters;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? configuration["Jwt:Secret"] ?? "dev-key-change-me")),
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractToken(context);
        if (!string.IsNullOrWhiteSpace(token))
        {
            var principal = ValidateToken(token);
            if (principal is not null)
            {
                context.User = principal;
            }
        }

        await _next(context);
    }

    private static string? ExtractToken(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var headerValue))
        {
            return null;
        }

        var value = headerValue.ToString();
        return value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? value["Bearer ".Length..].Trim()
            : null;
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            return _tokenHandler.ValidateToken(token, _validationParameters, out _);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
