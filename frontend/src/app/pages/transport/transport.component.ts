import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { TransportService } from "../../services/transport.service";
import type { TransportTrip } from "../../services/transport.service";
import { CardModule } from "primeng/card";
import { ButtonModule } from "primeng/button";
import { TagModule } from "primeng/tag";
import { DatePickerModule } from "primeng/datepicker";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { DividerModule } from "primeng/divider";
import { AccordionModule } from "primeng/accordion";
import { InputTextModule } from "primeng/inputtext";

@Component({
  selector: "app-transport",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    TagModule,
    DatePickerModule,
    ProgressSpinnerModule,
    DividerModule,
    AccordionModule,
    InputTextModule,
  ],
  templateUrl: "./transport.component.html",
  styleUrls: ["./transport.component.css"],
})
export class TransportComponent {
  from = "";
  to = "";

  departureTime: Date | null = null;
  arrivalTime: Date | null = null;
  minDateTime = new Date();
  timeValidationError = "";

  loading = false;

  trips: TransportTrip[] = [];

  constructor(private readonly transportService: TransportService) {}

  searchTrips(): void {
    if (!this.from.trim() || !this.to.trim()) {
      return;
    }

    this.minDateTime = new Date();
    this.timeValidationError = "";

    if (
      (this.departureTime && this.departureTime < this.minDateTime) ||
      (this.arrivalTime && this.arrivalTime < this.minDateTime)
    ) {
      this.timeValidationError =
        "Departure and arrival time cannot be earlier than now.";
      return;
    }

    this.loading = true;

    this.transportService
      .searchTrips(this.from, this.to, this.departureTime, this.arrivalTime)
      .subscribe({
        next: (trips) => {
          this.trips = trips;
          this.loading = false;
        },
        error: (error) => {
          console.error(error);
          this.loading = false;
        },
      });
  }

  onDatePickerOpen(): void {
    this.minDateTime = new Date();
  }

  swapLocations(): void {
    [this.from, this.to] = [this.to, this.from];
  }

  clearSearch(): void {
    this.from = "";
    this.to = "";

    this.departureTime = null;
    this.arrivalTime = null;
    this.timeValidationError = "";

    this.trips = [];
  }
}
