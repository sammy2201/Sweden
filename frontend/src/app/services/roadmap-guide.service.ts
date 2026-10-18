import { Injectable, inject } from "@angular/core";
import { GuideDirectoryService } from "./guide-directory.service";
import type { GuideLink } from "../models/guide.models";

/** Connect roadmap rule identifiers to guide routes that exist in the guide directory. */
const GUIDE_ROUTE_BY_TASK_RULE: Readonly<Record<string, string>> = {
  "personnummer-prepare": "/p-number",
  "personnummer-status": "/p-number",
  "personnummer-pending": "/p-number",
  "housing-search": "/housing",
  "housing-next": "/housing",
  "bank-account": "/banking",
  bankid: "/digital-id",
};

@Injectable({ providedIn: "root" })
export class RoadmapGuideService {
  private readonly guideDirectory = inject(GuideDirectoryService);

  forTaskRule(ruleId: string): GuideLink | null {
    const route = GUIDE_ROUTE_BY_TASK_RULE[ruleId];
    if (!route) return null;
    return this.guideDirectory.guides.find((guide) => guide.route === route) ?? null;
  }
}
