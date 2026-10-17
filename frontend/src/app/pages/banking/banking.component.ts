import {
  Component,
  DestroyRef,
  inject,
  signal,
  ViewEncapsulation,
} from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import type { Bank } from "./bank.model";
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
  private readonly destroyRef = inject(DestroyRef);
  readonly banks = signal<Bank[]>([]);
  readonly errorMessage = signal("");

  ngOnInit(): void {
    this.bankService
      .getBanks()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (banks) => {
          this.banks.set(banks);
          this.errorMessage.set("");
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            getApiErrorMessage(
              error,
              "We could not load the bank list right now.",
            ),
          );
        },
      });
  }
}
