import { Component, inject } from "@angular/core";
import { RouterLink, RouterOutlet } from "@angular/router";
import { MenuService } from "./services/menu.service";

@Component({
    selector: "app-root",
    imports: [RouterOutlet, RouterLink],
    templateUrl: "./app.component.html",
    styleUrl: "./app.component.css"
})
export class AppComponent {
  readonly menuService = inject(MenuService);
  readonly menuItems = this.menuService.getMenuItems();
}
