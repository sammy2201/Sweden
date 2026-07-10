import { Component, inject, ViewEncapsulation } from "@angular/core";
import type { Bank } from "./bank.model";
import { BankingIntroComponent } from "./components/banking-intro.component";
import { BankingBanksComponent } from "./components/banking-banks.component";
import { BankingSetupComponent } from "./components/banking-setup.component";
import { BankingCardsComponent } from "./components/banking-cards.component";
import { BankService } from "../../services/bank.service";

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
  private bankService = inject(BankService);
  banks: Bank[] = [];

  ngOnInit(): void {
    this.bankService.getBanks().subscribe({
      next: (banks) => {
        this.banks = banks;
      },
      error: (err) => {
        console.error("Failed to load banks", err);
      },
    });
  }
}
