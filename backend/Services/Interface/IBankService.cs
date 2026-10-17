namespace SwedenStart;

public interface IBankService
{
     IEnumerable<BankDto> GetBanks();
}
