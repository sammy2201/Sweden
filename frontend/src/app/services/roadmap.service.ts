import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";
import type {
  RoadmapQuestion,
  RoadmapRequest,
  RoadmapResponse,
} from "../models/roadmap.models";

@Injectable({ providedIn: "root" })
export class RoadmapService {
  constructor(private readonly http: HttpClient) {}

  getQuestions(): Observable<RoadmapQuestion[]> {
    return this.http.get<RoadmapQuestion[]>(`${environment.apiUrl}/roadmap/questions`);
  }

  getRoadmap(): Observable<RoadmapResponse | null> {
    return this.http.get<RoadmapResponse | null>(
      `${environment.apiUrl}/roadmap`,
    );
  }

  generateRoadmap(request: RoadmapRequest): Observable<RoadmapResponse> {
    return this.http.post<RoadmapResponse>(
      `${environment.apiUrl}/roadmap`,
      request,
    );
  }

  updateRoadmap(request: RoadmapRequest): Observable<RoadmapResponse> {
    return this.http.put<RoadmapResponse>(
      `${environment.apiUrl}/roadmap`,
      request,
    );
  }

  deleteRoadmap(): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/roadmap`);
  }

  updateTaskCompletion(
    taskId: string,
    completed: boolean,
  ): Observable<RoadmapResponse> {
    return this.http.patch<RoadmapResponse>(
      `${environment.apiUrl}/roadmap/tasks/${taskId}`,
      { completed },
    );
  }
}
