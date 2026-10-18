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
