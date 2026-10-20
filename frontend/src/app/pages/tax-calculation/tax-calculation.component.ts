import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { InputNumberModule } from "primeng/inputnumber";
import { CheckboxModule } from "primeng/checkbox";
import { ButtonModule } from "primeng/button";
import { SkeletonModule } from "primeng/skeleton";
import { SelectModule } from "primeng/select";
import { TaxService } from "../../services/tax.service";
import type {
  TaxCalculationRequest,
  TaxCalculationResponse,
} from "../../models/tax.models";
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
  municipalityError = "";

  constructor(private taxService: TaxService) {}

  async calculate(): Promise<void> {
    this.municipalityError = "";
    const selectedMunicipality = this.request.municipality?.trim();
    if (
      !selectedMunicipality ||
      !this.municipalities.includes(selectedMunicipality)
    ) {
      this.municipalityError =
        "Select a municipality from the list before calculating.";
      return;
    }

    this.loading = true;
    this.errorMessage = "";
    try {
      this.result = await this.taxService.calculateTax(this.request);
    } catch (error: unknown) {
      this.errorMessage = getApiErrorMessage(
        error,
        "We could not calculate your tax right now.",
      );
    } finally {
      this.loading = false;
    }
  }

  onMunicipalityChange(municipality: string | null): void {
    this.request.municipality = municipality ?? "";
    this.municipalityError = "";
  }

  municipalities: string[] = [];

  async ngOnInit(): Promise<void> {
    try {
      this.municipalities = await this.taxService.getMunicipalities();
    } catch (error: unknown) {
      this.errorMessage = getApiErrorMessage(
        error,
        "We could not load the municipality list right now.",
      );
    }
  }
}
