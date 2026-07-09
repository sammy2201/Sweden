import { Component } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";

@Component({
  selector: "app-banking-cards",
  standalone: true,
  imports: [ButtonModule, CardModule, TagModule],
  templateUrl: "./banking-cards.component.html",
})
export class BankingCardsComponent {
  openWebsite(url: string): void {
    globalThis.open(url, "_blank", "noopener,noreferrer");
  }
}
