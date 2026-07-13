using System.Globalization;

namespace SwedenStart;

public class TaxService : ITaxService
{
     private const int TaxYear = 2026;
     private const decimal PriceBaseAmount = 59_200m;
     private const decimal StateTaxableIncomeThreshold = 660_400m;
     private const decimal StateIncomeTaxRate = 20m;

     private readonly List<MunicipalityTaxRate> _rates;

     public TaxService(TaxDataProvider taxDataProvider)
     {
          _rates = taxDataProvider.TaxRates.ToList();

          if (_rates.Count == 0)
               throw new InvalidOperationException("Tax data source returned no municipality rates.");
     }

     public IEnumerable<MunicipalityTaxRate> GetTaxRates()
     {
          return _rates
               .GroupBy(rate => NormalizeMunicipality(rate.Municipality), StringComparer.OrdinalIgnoreCase)
               .Select(group => AggregateMunicipality(group))
               .OrderBy(rate => rate.Municipality, StringComparer.Create(new CultureInfo("sv-SE"), true));
     }

     public TaxCalculationResponse Calculate(TaxCalculationRequest request)
     {
          Validate(request);

          var annualGrossIncome = RoundDownToHundred(request.MonthlySalary * 12m);
          var rate = FindMunicipalityRate(request.Municipality);
          var olderTaxpayer = UsesEnhancedSeniorRules(request.Age);

          var basicDeduction = CalculateBasicDeduction(annualGrossIncome, olderTaxpayer);
          var taxableEarnedIncome = RoundDownToHundred(Math.Max(0m, annualGrossIncome - basicDeduction));

          var localTaxRate = rate.MunicipalTax + rate.RegionalTax;
          var municipalIncomeTax = taxableEarnedIncome * Percent(localTaxRate);
          var burialFee = taxableEarnedIncome * Percent(rate.BurialFee);
          var churchFee = request.ChurchMember
               ? taxableEarnedIncome * Percent(rate.ChurchFee)
               : 0m;
          var stateIncomeTax = Math.Max(0m, taxableEarnedIncome - StateTaxableIncomeThreshold) * Percent(StateIncomeTaxRate);

          var earnedIncomeTaxCredit = CalculateEarnedIncomeTaxCredit(
               annualGrossIncome,
               basicDeduction,
               localTaxRate,
               olderTaxpayer);

          var finalMunicipalIncomeTax = Math.Max(0m, municipalIncomeTax - earnedIncomeTaxCredit);
          var annualTax = RoundToKrona(finalMunicipalIncomeTax + burialFee + churchFee + stateIncomeTax);
          var monthlyTax = RoundMoney(annualTax / 12m);
          var monthlyNetSalary = RoundMoney(request.MonthlySalary - monthlyTax);
          var effectiveTaxRate = request.MonthlySalary == 0m
               ? 0m
               : RoundMoney(monthlyTax / request.MonthlySalary * 100m);

          return new TaxCalculationResponse
          {
               GrossSalary = RoundMoney(request.MonthlySalary),

               MunicipalTax = RoundMoney(finalMunicipalIncomeTax / 12m),
               StateTax = RoundMoney(stateIncomeTax / 12m),
               ChurchFee = RoundMoney(churchFee / 12m),
               TaxCredits = RoundMoney(-(earnedIncomeTaxCredit / 12m)),
               TotalTax = monthlyTax,

               TaxAmount = monthlyTax,
               NetSalary = monthlyNetSalary,

               EffectiveTaxRate = effectiveTaxRate,
               TaxRate = effectiveTaxRate,

               Municipality = rate.Municipality,
               TaxTable = rate.TaxTable
          };
     }

     private MunicipalityTaxRate FindMunicipalityRate(string municipality)
     {
          var normalized = NormalizeMunicipality(municipality);
          var matches = _rates
               .Where(rate => NormalizeMunicipality(rate.Municipality).Equals(normalized, StringComparison.OrdinalIgnoreCase))
               .ToList();

          if (matches.Count == 0)
               throw new KeyNotFoundException("Municipality not found.");

          return AggregateMunicipality(matches);
     }

     private static MunicipalityTaxRate AggregateMunicipality(IEnumerable<MunicipalityTaxRate> rates)
     {
          var items = rates.ToList();
          var first = items[0];
          var municipalTax = Average(items, rate => rate.MunicipalTax);
          var regionalTax = Average(items, rate => rate.RegionalTax);
          var burialFee = Average(items, rate => rate.BurialFee);
          var churchFee = items.Count == 1 ? items[0].ChurchFee : 0m;

          return new MunicipalityTaxRate
          {
               Year = first.Year == 0 ? TaxYear : first.Year,
               ParishCode = first.ParishCode,
               Municipality = ToDisplayMunicipality(first.Municipality),
               Parish = items.Count == 1 ? first.Parish : "AVERAGE",
               MunicipalTax = municipalTax,
               RegionalTax = regionalTax,
               BurialFee = burialFee,
               ChurchFee = churchFee,
               TotalTaxExcludingChurch = municipalTax + regionalTax + burialFee,
               TotalTaxIncludingChurch = municipalTax + regionalTax + burialFee + churchFee
          };
     }

     private static decimal CalculateBasicDeduction(decimal annualIncome, bool olderTaxpayer)
     {
          if (annualIncome <= 0m)
               return 0m;

          var deduction = olderTaxpayer
               ? CalculateEnhancedBasicDeduction(annualIncome)
               : CalculateStandardBasicDeduction(annualIncome);

          return Math.Min(annualIncome, RoundUpToHundred(deduction));
     }

     private static decimal CalculateStandardBasicDeduction(decimal annualIncome)
     {
          var pbb = PriceBaseAmount;

          if (annualIncome <= 0.99m * pbb)
               return 0.423m * pbb;

          if (annualIncome <= 2.72m * pbb)
               return (0.423m * pbb) + (0.20m * (annualIncome - (0.99m * pbb)));

          if (annualIncome <= 3.11m * pbb)
               return 0.77m * pbb;

          if (annualIncome <= 7.88m * pbb)
               return (0.77m * pbb) - (0.10m * (annualIncome - (3.11m * pbb)));

          return 0.293m * pbb;
     }

     private static decimal CalculateEnhancedBasicDeduction(decimal annualIncome)
     {
          var pbb = PriceBaseAmount;

          if (annualIncome <= 0.99m * pbb)
               return annualIncome;

          if (annualIncome <= 1.11m * pbb)
               return 0.99m * pbb;

          if (annualIncome <= 2.72m * pbb)
               return (0.99m * pbb) + (0.20m * (annualIncome - (1.11m * pbb)));

          if (annualIncome <= 3.11m * pbb)
               return 1.312m * pbb;

          if (annualIncome <= 7.88m * pbb)
               return (1.312m * pbb) - (0.02m * (annualIncome - (3.11m * pbb)));

          return 1.985m * pbb;
     }

     private static decimal CalculateEarnedIncomeTaxCredit(
          decimal annualWorkIncome,
          decimal basicDeduction,
          decimal municipalIncomeTaxRate,
          bool olderTaxpayer)
     {
          if (annualWorkIncome <= 0m || municipalIncomeTaxRate <= 0m)
               return 0m;

          return olderTaxpayer
               ? CalculateSeniorEarnedIncomeTaxCredit(annualWorkIncome, municipalIncomeTaxRate)
               : CalculateStandardEarnedIncomeTaxCredit(annualWorkIncome, basicDeduction, municipalIncomeTaxRate);
     }

     private static decimal CalculateStandardEarnedIncomeTaxCredit(
          decimal annualWorkIncome,
          decimal basicDeduction,
          decimal municipalIncomeTaxRate)
     {
          var pbb = PriceBaseAmount;
          decimal creditBase;

          if (annualWorkIncome <= 0.91m * pbb)
          {
               creditBase = annualWorkIncome;
          }
          else if (annualWorkIncome <= 3.24m * pbb)
          {
               creditBase = (0.91m * pbb) + (0.34m * (annualWorkIncome - (0.91m * pbb)));
          }
          else if (annualWorkIncome <= 8.08m * pbb)
          {
               creditBase = (1.7022m * pbb) + (0.128m * (annualWorkIncome - (3.24m * pbb)));
          }
          else
          {
               creditBase = 2.32172m * pbb;
          }

          return Math.Max(0m, creditBase - basicDeduction) * Percent(municipalIncomeTaxRate);
     }

     private static decimal CalculateSeniorEarnedIncomeTaxCredit(decimal annualWorkIncome, decimal municipalIncomeTaxRate)
     {
          decimal credit;

          if (annualWorkIncome <= 100_000m)
          {
               credit = 0.20m * annualWorkIncome;
          }
          else if (annualWorkIncome <= 300_000m)
          {
               credit = 20_000m + (0.05m * (annualWorkIncome - 100_000m));
          }
          else
          {
               credit = 30_000m;
          }

          var maximumMunicipalIncomeTax = annualWorkIncome * Percent(municipalIncomeTaxRate);
          return Math.Min(credit, maximumMunicipalIncomeTax);
     }

     private static bool UsesEnhancedSeniorRules(int age)
     {
          return age >= 66;
     }

     private static void Validate(TaxCalculationRequest request)
     {
          if (request == null)
               throw new ArgumentNullException(nameof(request));

          if (request.MonthlySalary <= 0m)
               throw new ArgumentException("Monthly salary must be greater than zero.");

          if (string.IsNullOrWhiteSpace(request.Municipality))
               throw new ArgumentException("Municipality is required.");

          if (request.Age < 0 || request.Age > 120)
               throw new ArgumentException("Age must be between 0 and 120.");
     }

     private static string NormalizeMunicipality(string value)
     {
          return value.Trim().Normalize().ToUpperInvariant();
     }

     private static string ToDisplayMunicipality(string value)
     {
          return CultureInfo.GetCultureInfo("sv-SE").TextInfo.ToTitleCase(value.Trim().ToLower(new CultureInfo("sv-SE")));
     }

     private static decimal Percent(decimal rate)
     {
          return rate / 100m;
     }

     private static decimal Average(IEnumerable<MunicipalityTaxRate> rates, Func<MunicipalityTaxRate, decimal> selector)
     {
          return rates.Select(selector).DefaultIfEmpty(0m).Average();
     }

     private static decimal RoundMoney(decimal amount)
     {
          return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
     }

     private static decimal RoundToKrona(decimal amount)
     {
          return decimal.Round(amount, 0, MidpointRounding.AwayFromZero);
     }

     private static decimal RoundDownToHundred(decimal amount)
     {
          return Math.Floor(amount / 100m) * 100m;
     }

     private static decimal RoundUpToHundred(decimal amount)
     {
          return Math.Ceiling(amount / 100m) * 100m;
     }
}
