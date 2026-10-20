import { Injectable } from "@angular/core";
import axios from "axios";
import { environment } from "../../environments/environment";
import { AuthService } from "./auth.service";
import type { Bank } from "../models/banking.models";

@Injectable({
  providedIn: "root",
})
export class BankService {
  constructor(private readonly authService: AuthService) {}

  async getBanks(): Promise<Bank[]> {
    try {
      const response = await axios.get<Bank[]>(
        `${environment.apiUrl}/bank-details`,
        this.requestConfig(),
      );
      return response.data;
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
