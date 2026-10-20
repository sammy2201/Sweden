import { Injectable } from "@angular/core";
import axios from "axios";
import { environment } from "../../environments/environment";
import { AuthService } from "./auth.service";
import type {
  RoadmapQuestion,
  RoadmapRequest,
  RoadmapResponse,
} from "../models/roadmap.models";

@Injectable({ providedIn: "root" })
export class RoadmapService {
  constructor(private readonly authService: AuthService) {}

  async getQuestions(): Promise<RoadmapQuestion[]> {
    try {
      const response = await axios.get<RoadmapQuestion[]>(
        `${environment.apiUrl}/roadmap/questions`,
        this.requestConfig(),
      );
      return response.data;
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async getRoadmap(): Promise<RoadmapResponse | null> {
    try {
      const response = await axios.get<RoadmapResponse | null>(
        `${environment.apiUrl}/roadmap`,
        this.requestConfig(),
      );
      return response.data;
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async generateRoadmap(request: RoadmapRequest): Promise<RoadmapResponse> {
    try {
      const response = await axios.post<RoadmapResponse>(
        `${environment.apiUrl}/roadmap`,
        request,
        this.requestConfig(),
      );
      return response.data;
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async updateRoadmap(request: RoadmapRequest): Promise<RoadmapResponse> {
    try {
      const response = await axios.put<RoadmapResponse>(
        `${environment.apiUrl}/roadmap`,
        request,
        this.requestConfig(),
      );
      return response.data;
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async deleteRoadmap(): Promise<void> {
    try {
      await axios.delete<void>(
        `${environment.apiUrl}/roadmap`,
        this.requestConfig(),
      );
    } catch (error: unknown) {
      this.handleUnauthorized(error);
    }
  }

  async updateTaskCompletion(
    taskId: string,
    completed: boolean,
  ): Promise<RoadmapResponse> {
    try {
      const response = await axios.patch<RoadmapResponse>(
        `${environment.apiUrl}/roadmap/tasks/${taskId}`,
        { completed },
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
