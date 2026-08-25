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
  title: string;
  description: string;
  completed: boolean;
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
}
