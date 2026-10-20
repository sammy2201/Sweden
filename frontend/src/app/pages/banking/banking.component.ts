import { Component, inject, signal, ViewEncapsulation } from "@angular/core";
import type { Bank } from "../../models/banking.models";
import { BankingIntroComponent } from "./components/banking-intro.component";
import { BankingBanksComponent } from "./components/banking-banks.component";
import { BankingSetupComponent } from "./components/banking-setup.component";
import { BankingCardsComponent } from "./components/banking-cards.component";
import { BankService } from "../../services/bank.service";
import { getApiErrorMessage } from "../../utils/api-error";

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
  readonly banks = signal<Bank[]>([]);
  readonly errorMessage = signal("");

  async ngOnInit(): Promise<void> {
    try {
      this.banks.set(await this.bankService.getBanks());
    } catch (error: unknown) {
      this.errorMessage.set(
        getApiErrorMessage(error, "We could not load the bank list right now."),
      );
    }
  }
}
