import { Injectable } from "@angular/core";
import type { GuideLink } from "../models/guide.models";

const GUIDE_LINKS: GuideLink[] = [
  {
    title: "Personal number",
    description:
      "Learn how registration works and what to prepare before applying.",
    icon: "pi pi-id-card",
    route: "/p-number",
    keywords: "personnummer skatteverket identity number registration",
    category: "Registration",
    essential: true,
    layout: "feature",
  },
  {
    title: "Housing",
    description:
      "Compare rental options, understand contracts, and avoid common scams.",
    icon: "pi pi-home",
    route: "/housing",
    keywords: "rent apartment home accommodation landlord contract",
    category: "Finding a home",
    essential: true,
    layout: "compact",
  },
  {
    title: "Digital identity",
    description:
      "Get oriented with e-identification and essential online services.",
    icon: "pi pi-mobile",
    route: "/digital-id",
    keywords: "bankid e-id online login digital services",
    category: "Everyday services",
    essential: false,
    layout: "compact",
  },
  {
    title: "Banking",
    description:
      "See what banks commonly ask for and how Swedish payment services fit in.",
    icon: "pi pi-wallet",
    route: "/banking",
    keywords: "bank account card swish payments money",
    category: "Money",
    essential: true,
    layout: "feature",
  },
  {
    title: "Taxes",
    description:
      "Read about Swedish taxes and estimate income tax by municipality.",
    icon: "pi pi-calculator",
    route: "/tax-info",
    keywords: "tax salary municipality income tax skatteverket",
    category: "Work and money",
    essential: false,
    layout: "feature",
  },
  {
    title: "Transport",
    description: "Plan public-transport journeys between Swedish cities.",
    icon: "pi pi-directions",
    route: "/transport",
    keywords: "train bus trip travel journey public transport",
    category: "Getting around",
    essential: false,
    layout: "compact",
  },
  {
    title: "Explore Sweden",
    description: "Browse counties and discover places across the country.",
    icon: "pi pi-compass",
    route: "/explore",
    keywords: "counties attractions places visit travel",
    category: "Discover",
    essential: false,
    layout: "feature",
  },
];

@Injectable({ providedIn: "root" })
export class GuideDirectoryService {
  readonly guides = GUIDE_LINKS;
  readonly essentials = GUIDE_LINKS.filter((guide) => guide.essential);

  search(query: string): GuideLink[] {
    const normalizedQuery = query.trim().toLocaleLowerCase();
    if (!normalizedQuery) {
      return [...this.guides];
    }

    return this.guides.filter((guide) =>
      `${guide.title} ${guide.description} ${guide.keywords} ${guide.category}`
        .toLocaleLowerCase()
        .includes(normalizedQuery),
    );
  }
}
