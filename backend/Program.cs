using Microsoft.Extensions.Configuration;
using Npgsql;
using Scalar.AspNetCore;
using SwedenStart.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IRoadmapRepository, RoadmapRepository>();
builder.Services.AddSingleton<IRoadmapService, RoadmapService>();
builder.Services.AddSingleton<IHealthService, HealthService>();
builder.Services.AddSingleton(sp => DbSettings.FromConfiguration(builder.Configuration));
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<DbSettings>();
    var connectionStringBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = settings.Host,
        Port = settings.Port,
        Database = settings.Database,
        Username = settings.Username,
        Password = settings.Password,
        SslMode = SslMode.Disable,
    };

    return NpgsqlDataSource.Create(connectionStringBuilder.ConnectionString);
});

var app = builder.Build();
var dbSettings = app.Services.GetRequiredService<DbSettings>();

app.Logger.LogInformation(
    "Starting Sweden Start API. PostgreSQL target: {Host}:{Port}/{Database}",
    dbSettings.Host,
    dbSettings.Port,
    dbSettings.Database);

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

sealed record DbSettings(string Host, int Port, string Database, string Username, string Password)
{
    public static DbSettings FromConfiguration(IConfiguration configuration)
    {
        var host = GetSetting(configuration, "POSTGRES_HOST", "localhost");
        var port = int.TryParse(GetSetting(configuration, "POSTGRES_PORT", "5432"), out var parsedPort)
            ? parsedPort
            : 5432;
        var database = GetSetting(configuration, "POSTGRES_DB", "sweden_start");
        var username = GetSetting(configuration, "POSTGRES_USER", "sweden_start");
        var password = GetSetting(configuration, "POSTGRES_PASSWORD", "sweden_start_dev");

        return new DbSettings(host, port, database, username, password);
    }

    private static string GetSetting(IConfiguration configuration, string key, string fallback)
        => configuration[key] ?? Environment.GetEnvironmentVariable(key) ?? fallback;
}


