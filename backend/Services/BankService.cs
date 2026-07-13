namespace SwedenStart;

using System.Text.Json;

public class BankService : IBankService
{
     private readonly IWebHostEnvironment _environment;
     private readonly Lazy<List<BankDto>> _banks;

     public BankService(IWebHostEnvironment environment)
     {
          _environment = environment;
          _banks = new Lazy<List<BankDto>>(LoadBanks, LazyThreadSafetyMode.ExecutionAndPublication);
     }

     public async Task<IEnumerable<BankDto>> GetBanksAsync()
     {
          return await Task.FromResult(_banks.Value);
     }

     private List<BankDto> LoadBanks()
     {
          var path = Path.Combine(_environment.ContentRootPath, "Data", "banks.json");

          if (!File.Exists(path))
          {
               return new List<BankDto>();
          }

          var json = File.ReadAllText(path);

          return JsonSerializer.Deserialize<List<BankDto>>(
                     json,
                     new JsonSerializerOptions(JsonSerializerDefaults.Web))
                 ?? new List<BankDto>();
     }
}