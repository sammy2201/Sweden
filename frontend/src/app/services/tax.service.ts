import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
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
  netSalary: number;
  effectiveTaxRate: number;
  municipality: string;
  taxTable: number;
}

interface MunicipalityTaxRateResponse {
  municipality: string;
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

  getMunicipalities(): Observable<string[]> {
    const url = `${environment.apiUrl}/tax/rates`;
    return this.http
      .get<MunicipalityTaxRateResponse[]>(url)
      .pipe(
        map((rates) =>
          Array.from(
            new Set(
              rates
                .map((rate) => rate.municipality?.trim())
                .filter((municipality): municipality is string =>
                  Boolean(municipality),
                ),
            ),
          ).sort((a, b) => a.localeCompare(b)),
        ),
      );
  }
}
