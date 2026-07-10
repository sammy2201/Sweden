namespace SwedenStart;

public class BankDto
{
     public string Name { get; set; } = string.Empty;
     public string Website { get; set; } = string.Empty;

     public bool ProvidesBankId { get; set; }
     public bool ProvidesSwish { get; set; }

     public bool StudentFriendly { get; set; }
     public bool EnglishSupport { get; set; }

     public bool DebitCard { get; set; }
     public bool CreditCard { get; set; }

     public bool ApplePay { get; set; }
     public bool GoogleWallet { get; set; }
     public bool SamsungWallet { get; set; }

     public bool PhysicalBranches { get; set; }
     public bool MobileApp { get; set; }

     public bool AccountWithoutPersonnummer { get; set; }
     public bool BusinessAccounts { get; set; }

     public string Fee { get; set; } = string.Empty;
     public string Notes { get; set; } = string.Empty;
}