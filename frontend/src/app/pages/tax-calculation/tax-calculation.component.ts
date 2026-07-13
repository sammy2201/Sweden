import { Component } from "@angular/core";
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

  constructor(private taxService: TaxService) {}

  calculate(): void {
    this.loading = true;
    this.taxService.calculateTax(this.request).subscribe({
      next: (res) => {
        this.result = res;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      },
    });
  }

  municipalities: string[] = [];

  ngOnInit(): void {
    this.taxService.getMunicipalities().subscribe({
      next: (res) => {
        this.municipalities = res;
      },
      error: (err) => {
        console.error(err);
      },
    });
  }
}
