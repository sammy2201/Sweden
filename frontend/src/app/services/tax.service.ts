import { Injectable } from "@angular/core";
import axios from "axios";
import { environment } from "../../environments/environment";
import { AuthService } from "./auth.service";
import type {
  MunicipalityTaxRateResponse,
  TaxCalculationRequest,
  TaxCalculationResponse,
} from "../models/tax.models";

@Injectable({
  providedIn: "root",
})
export class TaxService {
  constructor(private readonly authService: AuthService) {}

  async calculateTax(
    request: TaxCalculationRequest,
  ): Promise<TaxCalculationResponse> {
    try {
      const response = await axios.post<TaxCalculationResponse>(
        `${environment.apiUrl}/tax/calculate`,
        request,
        this.requestConfig(),
      );
      return response.data;
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async getMunicipalities(): Promise<string[]> {
    try {
      const response = await axios.get<MunicipalityTaxRateResponse[]>(
        `${environment.apiUrl}/tax/rates`,
        this.requestConfig(),
      );
      return Array.from(
        new Set(
          response.data
            .map((rate) => rate.municipality?.trim())
            .filter((municipality): municipality is string =>
              Boolean(municipality),
            ),
        ),
      ).sort((firstMunicipality, secondMunicipality) =>
        firstMunicipality.localeCompare(secondMunicipality),
      );
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  private requestConfig() {
    const token = this.authService.getValidAccessToken();
    return token ? { headers: { Authorization: `Bearer ${token}` } } : {};
  }

  private handleUnauthorized(error: unknown): never {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      this.authService.logout();
    }
    throw error;
  }
}
