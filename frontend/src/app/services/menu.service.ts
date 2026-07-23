import { Injectable } from "@angular/core";

export interface MenuItem {
  label: string;
  path: string;
  icon?: string;
}

@Injectable({
  providedIn: "root",
})
export class MenuService {
  private readonly menuItems: MenuItem[] = [
    { label: "Home", path: "/" },
    { label: "P Number", path: "/p-number" },
    { label: "Housing", path: "/housing" },
    { label: "Explore", path: "/explore" },
    { label: "Roadmap", path: "/roadmap" },
    { label: "Digital ID", path: "/digital-id" },
    { label: "Banking", path: "/banking" },
    { label: "Tax Info", path: "/tax-info" },
    { label: "Transport", path: "/transport" },
  ];

  getMenuItems(): MenuItem[] {
    return [...this.menuItems];
  }

  getMenuItem(path: string): MenuItem | undefined {
    return this.menuItems.find((item) => item.path === path);
  }
}
