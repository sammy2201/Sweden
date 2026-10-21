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
