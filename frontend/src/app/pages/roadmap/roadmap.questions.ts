export interface FlowQuestion {
  id: string;
  title: string;
  type?: "select" | "radio";
  options: string[];
  showIf?: (answers: Record<string, string>) => boolean;
}

const YES_NO = ["Yes", "No"];

export const QUESTIONS: FlowQuestion[] = [
  {
    id: "origin",
    title: "Where are you from?",
    options: ["EU", "Non-EU"],
  },

  {
    id: "residencePermit",
    title: "Do you already have a residence permit?",
    options: YES_NO,
    showIf: (a) => a["origin"] === "Non-EU",
  },

  {
    id: "liveInSweden",
    title: "Do you already live in Sweden?",
    options: YES_NO,
    showIf: (a) => a["origin"] === "EU" || a["residencePermit"] === "Yes",
  },

  {
    id: "personnummer",
    title: "Do you have a Personnummer?",
    options: YES_NO,
    showIf: (a) => a["liveInSweden"] === "Yes",
  },

  {
    id: "appliedPersonnummer",
    title: "Have you applied for a Personnummer?",
    options: ["Yes", "No", "Waiting for decision"],
    showIf: (a) => a["personnummer"] === "No",
  },

  {
    id: "idCard",
    title: "Do you have a Swedish ID card?",
    options: YES_NO,
    showIf: (a) => a["personnummer"] === "Yes",
  },

  {
    id: "bankAccount",
    title: "Do you have a Swedish bank account?",
    options: YES_NO,
    showIf: (a) => a["idCard"] === "Yes",
  },

  {
    id: "bankid",
    title: "Do you have BankID?",
    options: YES_NO,
    showIf: (a) => a["bankAccount"] === "Yes",
  },

  {
    id: "housing",
    title: "What is your housing situation?",
    options: ["Permanent", "Temporary", "Still looking"],
  },

  {
    id: "planToDrive",
    title: "Do you plan to drive in Sweden?",
    options: YES_NO,
  },

  {
    id: "drivingLicenceType",
    title: "Which licence do you have?",
    options: ["Swedish", "EU", "Other"],
    showIf: (a) => a["planToDrive"] === "Yes",
  },

  {
    id: "purpose",
    title: "Why are you in Sweden?",
    options: ["Work", "Study", "Looking for work", "Family"],
  },

  {
    id: "insurance",
    title: "Have you registered with Försäkringskassan?",
    options: YES_NO,
    showIf: (a) => a["liveInSweden"] === "Yes",
  },
];
