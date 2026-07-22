import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";

import { CardModule } from "primeng/card";
import { ButtonModule } from "primeng/button";
import { TagModule } from "primeng/tag";
import { AccordionModule } from "primeng/accordion";
import { InputTextModule } from "primeng/inputtext";

import {
  TransportService,
  TransportTrip,
} from "../../services/transport.service";

@Component({
  selector: "app-transport",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    TagModule,
    AccordionModule,
    InputTextModule,
  ],
  templateUrl: "./transport.component.html",
  styleUrls: ["./transport.component.css"],
})
export class TransportComponent {
  from = "";
  to = "";

  trips: TransportTrip[] = [];

  loading = false;

  constructor(private readonly transportService: TransportService) {}

  searchTrips(): void {
    if (!this.from.trim() || !this.to.trim()) {
      return;
    }

    this.loading = true;

    this.transportService.searchTrips(this.from, this.to).subscribe({
      next: (response) => {
        this.trips = response;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }
}
