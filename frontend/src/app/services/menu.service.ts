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
    { label: "Roadmap", path: "/roadmap" },
  ];

  getMenuItems(): MenuItem[] {
    return [...this.menuItems];
  }

  getMenuItem(path: string): MenuItem | undefined {
    return this.menuItems.find((item) => item.path === path);
  }
}
