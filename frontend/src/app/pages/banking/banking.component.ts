import { Component, ViewEncapsulation } from "@angular/core";
import type { Bank } from "./bank.model";
import { BANKS } from "./bank.data";
import { BankingIntroComponent } from "./components/banking-intro.component";
import { BankingBanksComponent } from "./components/banking-banks.component";
import { BankingSetupComponent } from "./components/banking-setup.component";
import { BankingCardsComponent } from "./components/banking-cards.component";

@Component({
  selector: "app-banking",
  standalone: true,
  imports: [
    BankingIntroComponent,
    BankingBanksComponent,
    BankingSetupComponent,
    BankingCardsComponent,
  ],
  templateUrl: "./banking.component.html",
  styleUrl: "./banking.component.css",
  encapsulation: ViewEncapsulation.None,
})
export class BankingComponent {
  progress = 42;

  userName = "Sanmay";

  banks: Bank[] = BANKS;
}
