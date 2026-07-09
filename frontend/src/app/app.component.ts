import { Component, inject } from "@angular/core";
import {
  Router,
  RouterLink,
  RouterOutlet,
  RouterLinkActive,
} from "@angular/router";
import { MenuService } from "./services/menu.service";
import { AuthService } from "./services/auth.service";

@Component({
  selector: "app-root",
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: "./app.component.html",
  styleUrl: "./app.component.css",
})
export class AppComponent {
  readonly menuService = inject(MenuService);
  readonly authService = inject(AuthService);
  readonly router = inject(Router);
  readonly menuItems = this.menuService.getMenuItems();

  logout(): void {
    this.authService.logout();
    this.router.navigate(["/login"]);
  }
}
