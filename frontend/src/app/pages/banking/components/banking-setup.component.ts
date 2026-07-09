import { Component } from "@angular/core";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";

@Component({
  selector: "app-banking-setup",
  standalone: true,
  imports: [CardModule, TagModule],
  templateUrl: "./banking-setup.component.html",
})
export class BankingSetupComponent {}
