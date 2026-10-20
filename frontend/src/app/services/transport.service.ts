import { Injectable } from "@angular/core";
import axios from "axios";
import { environment } from "../../environments/environment";
import { AuthService } from "./auth.service";
import type { TransportTrip } from "../models/transport.models";

@Injectable({
  providedIn: "root",
})
export class TransportService {
  constructor(private readonly authService: AuthService) {}

  async searchTrips(
    from: string,
    to: string,
    departureTime?: Date | null,
    arrivalTime?: Date | null,
  ): Promise<TransportTrip[]> {
    const params = {
      from,
      to,
      ...(departureTime ? { departureTime: departureTime.toISOString() } : {}),
      ...(arrivalTime ? { arrivalTime: arrivalTime.toISOString() } : {}),
    };

    try {
      const response = await axios.get<TransportTrip[]>(
        `${environment.apiUrl}/transport/search`,
        { ...this.requestConfig(), params },
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
