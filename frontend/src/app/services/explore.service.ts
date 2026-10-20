import { Injectable } from "@angular/core";
import axios from "axios";
import { environment } from "../../environments/environment";
import { AuthService } from "./auth.service";
import {
  Attraction,
  AttractionDetail,
  County,
  ExploreAttractionDetailResponse,
  ExploreAttractionsResponse,
  ExploreCountiesResponse,
  PagedResponse,
} from "../models/explore.models";

@Injectable({
  providedIn: "root",
})
export class ExploreService {
  constructor(private readonly authService: AuthService) {}

  async getCounties(): Promise<County[]> {
    try {
      const response = await axios.get<ExploreCountiesResponse>(
        `${environment.apiUrl}/explore/counties`,
        this.requestConfig(),
      );
      return response.data.counties ?? [];
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async getCounty(countyId: string): Promise<County | undefined> {
    const counties = await this.getCounties();
    return counties.find((county) => county.id === countyId);
  }

  async getAttractions(
    county: string,
    page = 1,
    pageSize = 12,
    category?: string,
  ): Promise<PagedResponse<Attraction>> {
    const cleanedCategory = category?.trim();
    const params = {
      county,
      page,
      pageSize,
      ...(cleanedCategory ? { category: cleanedCategory } : {}),
    };

    try {
      const response = await axios.get<ExploreAttractionsResponse>(
        `${environment.apiUrl}/explore/attractions`,
        { ...this.requestConfig(), params },
      );
      return {
        items: response.data.attractions ?? [],
        page: response.data.page,
        pageSize: response.data.pageSize,
        totalCount: response.data.totalCount,
        totalPages: response.data.totalPages,
        hasNextPage: response.data.hasNextPage,
        hasPreviousPage: response.data.hasPreviousPage,
      };
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async getAttraction(
    attractionId: string,
  ): Promise<AttractionDetail | undefined> {
    try {
      const response = await axios.get<ExploreAttractionDetailResponse>(
        `${environment.apiUrl}/explore/attractions/${encodeURIComponent(attractionId)}`,
        this.requestConfig(),
      );
      return response.data.attraction;
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
