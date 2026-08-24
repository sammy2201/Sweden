using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SwedenStart;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
     public AppDbContext CreateDbContext(string[] args)
     {
          var basePath = Directory.GetCurrentDirectory();

          var configuration = new ConfigurationBuilder()
              .SetBasePath(basePath)
              .AddJsonFile("appsettings.json", optional: true)
              .AddJsonFile("appsettings.Development.json", optional: true)
              .AddEnvironmentVariables()
              .Build();

          var connectionString = configuration.GetConnectionString("DefaultConnection");

          if (string.IsNullOrWhiteSpace(connectionString))
          {
               throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
          }

          var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
          optionsBuilder.UseNpgsql(connectionString);

          return new AppDbContext(optionsBuilder.Options);
     }
}
