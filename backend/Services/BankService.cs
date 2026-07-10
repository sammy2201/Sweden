namespace SwedenStart;

using System.Text.Json;

public class BankService : IBankService
{
     private readonly IWebHostEnvironment _environment;

     public BankService(IWebHostEnvironment environment)
     {
          _environment = environment;
     }

     public async Task<IEnumerable<BankDto>> GetBanksAsync()
     {
          var path = Path.Combine(
              _environment.ContentRootPath,
              "Data",
              "banks.json");

          Console.WriteLine(path);
          var json = await File.ReadAllTextAsync(path);

          return JsonSerializer.Deserialize<List<BankDto>>(
    json,
    new JsonSerializerOptions(JsonSerializerDefaults.Web))
    ?? new List<BankDto>();
     }
}