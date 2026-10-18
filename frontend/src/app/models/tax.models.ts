export interface TaxCalculationRequest {
  monthlySalary: number;
  municipality: string;
  age: number;
  churchMember: boolean;
}

export interface TaxCalculationResponse {
  grossSalary: number;
  municipalTax: number;
  stateTax: number;
  churchFee: number;
  taxCredits: number;
  totalTax: number;
  netSalary: number;
  effectiveTaxRate: number;
  municipality: string;
  taxTable: number;
}

export interface MunicipalityTaxRateResponse {
  municipality: string;
}
