using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Scalar.AspNetCore;
using System.Text;
using SwedenStart;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Auth services and repos
builder.Services.AddScoped<IAuthRepo, AuthRepo>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "sweden-start",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "sweden-start-audience",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "dev-key-change-me"))
    };
});
builder.Services.AddScoped<IRoadmapRepository, RoadmapRepository>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddSingleton<IHealthService, HealthService>();

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

var connectionStringBuilder = new NpgsqlConnectionStringBuilder(defaultConnection);

builder.Services.AddSingleton(sp => NpgsqlDataSource.Create(defaultConnection));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));

var app = builder.Build();

app.Logger.LogInformation(
    "Starting Sweden Start API. PostgreSQL target: {Host}:{Port}/{Database}",
    connectionStringBuilder.Host,
    connectionStringBuilder.Port,
    connectionStringBuilder.Database);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sweden Start API");
    });
    app.MapGet("/docs", () => Results.Redirect("/scalar"));
    app.MapGet("/swager", () => Results.Redirect("/scalar"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        using var connection = app.Services.GetRequiredService<NpgsqlDataSource>().CreateConnection();
        connection.Open();
        app.Logger.LogInformation("PostgreSQL connection established successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "PostgreSQL is not reachable yet. The API will continue starting.");
    }
});

app.MapControllers();

app.Run();


