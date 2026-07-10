namespace SwedenStart;

public interface IBankService
{
     Task<IEnumerable<BankDto>> GetBanksAsync();
}