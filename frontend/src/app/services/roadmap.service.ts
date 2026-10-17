import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";

export interface RoadmapRequest {
  origin: string;
  residencePermit: string;
  liveInSweden: string;
  personnummer: string;
  appliedPersonnummer: string;
  idCard: string;
  bankAccount: string;
  bankId: string;
  housing: string;
  planToDrive: string;
  drivingLicenceType: string;
  purpose: string;
  insurance: string;
}

export interface RoadmapTaskResponse {
  id: string;
  title: string;
  description: string;
  completed: boolean;
  ruleId: string;
  category: string;
  priority: number;
  state: string;
  stateBeforeCompletion: string;
  resourceUrl?: string | null;
  ruleExplanation: string;
  prerequisites: { ruleId: string; explanation: string }[];
}

export interface RoadmapQuestionDependency {
  id: string;
  sourceQuestionId: keyof RoadmapRequest;
  operator: "Equals";
  answer: string;
  dependentQuestionId: keyof RoadmapRequest;
  effect: "Show" | "Hide" | "RequireClarification";
  explanation: string;
}

export interface RoadmapQuestion {
  id: keyof RoadmapRequest;
  text: string;
  answerType: "select" | "radio";
  options: { id: string; value: string; label: string }[];
  required: boolean;
  order: number;
  dependencies: RoadmapQuestionDependency[];
  guidance?: string | null;
}

export interface RoadmapResponse {
  id: string;
  userId: string;
  origin: string;
  residencePermit: string;
  liveInSweden: string;
  personnummer: string;
  appliedPersonnummer: string;
  idCard: string;
  bankAccount: string;
  bankId: string;
  housing: string;
  planToDrive: string;
  drivingLicenceType: string;
  purpose: string;
  insurance: string;
  tasks: RoadmapTaskResponse[];
}

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
