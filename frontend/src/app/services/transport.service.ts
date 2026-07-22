import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";

export interface TransportStation {
  name: string;
  stationId: string;
}

export interface TransportTripLeg {
  operator: string;
  transportType: string;
  line: string;
  from: string;
  to: string;
  departureTime: string;
  arrivalTime: string;
  originPlatform: string;
  destinationPlatform: string;
  direction: string;
  isCancelled: boolean;
}

export interface TransportTrip {
  operator: string;
  departureTime: string;
  arrivalTime: string;
  duration: string;
  numberOfChanges: number;
  isDirect: boolean;
  transportType: string;
  originPlatform: string;
  destinationPlatform: string;
  direction: string;
  isCancelled: boolean;
  origin: TransportStation;
  destination: TransportStation;
  legs: TransportTripLeg[];
}

@Injectable({
  providedIn: "root",
})
export class TransportService {
  constructor(private readonly http: HttpClient) {}

  searchTrips(from: string, to: string): Observable<TransportTrip[]> {
    const params = new HttpParams().set("from", from).set("to", to);

    return this.http.get<TransportTrip[]>(
      `${environment.apiUrl}/transport/search`,
      { params },
    );
  }
}
