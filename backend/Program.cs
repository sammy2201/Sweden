using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Npgsql;
using Scalar.AspNetCore;
using System.Text;
using SwedenStart;

var builder = WebApplication.CreateBuilder(args);


var jwtKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt:Secret"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT key is not configured.");
}

builder.Configuration["Jwt:Key"] = jwtKey;
builder.Configuration["Jwt:Secret"] = jwtKey;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Enter 'Bearer' followed by your JWT token."
        };

        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", null)] = []
            }
        ];

        return Task.CompletedTask;
    });
});

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    options.Events = new JwtBearerEvents();
});
builder.Services.AddScoped<IRoadmapRepository, RoadmapRepository>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddSingleton<IHealthService, HealthService>();
builder.Services.AddSingleton<TaxDataProvider>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddHttpClient<ITransportService, TransportService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["ResRobot:BaseUrl"] ?? "https://api.resrobot.se/v2.1/";

    if (!baseUrl.EndsWith('/'))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(20);
});


var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

var connectionStringBuilder = new NpgsqlConnectionStringBuilder(defaultConnection);
var redactedConnectionStringBuilder = new NpgsqlConnectionStringBuilder(defaultConnection)
{
    Password = string.IsNullOrWhiteSpace(connectionStringBuilder.Password) ? string.Empty : "***"
};

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
        options.AddHttpAuthentication("Bearer", scheme =>
        {
            scheme.Token = string.Empty;
        });
        options.AddPreferredSecuritySchemes(["Bearer"]);
        options.EnablePersistentAuthentication();
    });
    app.MapGet("/docs", () => Results.Redirect("/scalar"));
    app.MapGet("/swager", () => Results.Redirect("/scalar"));
}

app.UseHttpsRedirection();

app.UseCors("frontend");
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

