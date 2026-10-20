export interface GuideLink {
  title: string;
  description: string;
  icon: string;
  route: string;
  keywords: string;
  category: string;
  essential: boolean;
  layout: "compact" | "feature";
}
