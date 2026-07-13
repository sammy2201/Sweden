import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";

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
  taxAmount: number;
  netSalary: number;
  effectiveTaxRate: number;
  taxRate: number;
  municipality: string;
  taxTable: number;
}

@Injectable({
  providedIn: "root",
})
export class TaxService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/tax/calculate`;

  calculateTax(
    request: TaxCalculationRequest,
  ): Observable<TaxCalculationResponse> {
    return this.http.post<TaxCalculationResponse>(this.apiUrl, request);
  }
}
