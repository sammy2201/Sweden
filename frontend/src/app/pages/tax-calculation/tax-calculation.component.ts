import { Component, DestroyRef, inject } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { InputNumberModule } from "primeng/inputnumber";
import { CheckboxModule } from "primeng/checkbox";
import { ButtonModule } from "primeng/button";
import { SkeletonModule } from "primeng/skeleton";
import { SelectModule } from "primeng/select";
import {
  TaxService,
  TaxCalculationRequest,
  TaxCalculationResponse,
} from "../../services/tax.service";
import { getApiErrorMessage } from "../../utils/api-error";

@Component({
  selector: "app-tax-calculation",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InputNumberModule,
    CheckboxModule,
    ButtonModule,
    SkeletonModule,
    SelectModule,
  ],
  templateUrl: "./tax-calculation.component.html",
  styleUrls: ["./tax-calculation.component.css"],
})
export class TaxCalculationComponent {
  request: TaxCalculationRequest = {
    monthlySalary: 42000,
    municipality: "Malmö",
    age: 25,
    churchMember: false,
  };
  loading = false;
  result: TaxCalculationResponse | null = null;
  errorMessage = "";

  private readonly destroyRef = inject(DestroyRef);

  constructor(private taxService: TaxService) {}

  calculate(): void {
    this.loading = true;
    this.errorMessage = "";
    this.taxService
      .calculateTax(this.request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.result = res;
          this.loading = false;
        },
        error: (error: unknown) => {
          this.errorMessage = getApiErrorMessage(
            error,
            "We could not calculate your tax right now.",
          );
          this.loading = false;
        },
      });
  }

  municipalities: string[] = [];

  ngOnInit(): void {
    this.taxService
      .getMunicipalities()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.municipalities = res;
        },
        error: (error: unknown) => {
          this.errorMessage = getApiErrorMessage(
            error,
            "We could not load the municipality list right now.",
          );
        },
      });
  }
}
