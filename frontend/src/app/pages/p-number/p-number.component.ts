import { Component, inject } from "@angular/core";
import { DomSanitizer, SafeResourceUrl } from "@angular/platform-browser";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";
import { TimelineModule } from "primeng/timeline";
import { AccordionModule } from "primeng/accordion";
import { PanelModule } from "primeng/panel";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { CommonModule } from "@angular/common";

@Component({
    selector: "app-p-number",
    templateUrl: "./p-number.component.html",
    styleUrl: "./p-number.component.css",
    imports: [
        CommonModule,
        CardModule,
        TagModule,
        TimelineModule,
        AccordionModule,
        PanelModule,
        ButtonModule,
        InputTextModule,
    ]
})
export class PNumberComponent {
  private readonly sanitizer = inject(DomSanitizer);

  steps = [
    {
      title: "Check Eligibility",
      description:
        "You normally need to be moving to Sweden for at least one year.",
    },
    {
      title: "Prepare Documents",
      description:
        "Bring your passport, residence permit and supporting documents.",
    },
    {
      title: "Visit Skatteverket",
      description: "Submit your application and complete the identity check.",
    },
    {
      title: "Receive Personnummer",
      description:
        "Once approved, your personal identity number will be issued.",
    },
  ];

  documents = [
    "Passport or national ID",
    "Residence permit",
    "Employment contract or university admission",
    "Marriage or birth certificates if applicable",
    "Proof of address",
  ];

  isLocating = false;
  currentSearchLabel = "Sweden";
  locationStatus = "Search by city, address, or use your current location.";
  externalMapUrl = this.createExternalMapUrl(
    "Skatteverket servicekontor Sweden",
  );
  mapUrl: SafeResourceUrl = this.createEmbedMapUrl(
    "Skatteverket servicekontor Sweden",
  );

  searchByPlace(place: string): void {
    const trimmedPlace = place.trim();

    if (!trimmedPlace) {
      this.locationStatus =
        "Enter a city, postcode, or address to search nearby offices.";
      return;
    }

    this.updateMap(
      `Skatteverket servicekontor near ${trimmedPlace}, Sweden`,
      trimmedPlace,
    );
    this.locationStatus = `Showing nearby Skatteverket service offices for ${trimmedPlace}.`;
  }

  useCurrentLocation(): void {
    if (typeof navigator === "undefined" || !navigator.geolocation) {
      this.locationStatus =
        "Your browser does not support location search. Try typing a city instead.";
      return;
    }

    this.isLocating = true;
    this.locationStatus = "Finding your location...";

    navigator.geolocation.getCurrentPosition(
      ({ coords }) => {
        const location = `${coords.latitude},${coords.longitude}`;
        this.updateMap(
          `Skatteverket servicekontor near ${location}`,
          "your current location",
        );
        this.locationStatus =
          "Showing nearby Skatteverket service offices from your current location.";
        this.isLocating = false;
      },
      () => {
        this.locationStatus =
          "Location access was not allowed. Search by city or address instead.";
        this.isLocating = false;
      },
      {
        enableHighAccuracy: true,
        maximumAge: 300000,
        timeout: 10000,
      },
    );
  }

  private updateMap(searchQuery: string, label: string): void {
    this.currentSearchLabel = label;
    this.externalMapUrl = this.createExternalMapUrl(searchQuery);
    this.mapUrl = this.createEmbedMapUrl(searchQuery);
  }

  private createEmbedMapUrl(searchQuery: string): SafeResourceUrl {
    const encodedQuery = encodeURIComponent(searchQuery);
    return this.sanitizer.bypassSecurityTrustResourceUrl(
      `https://www.google.com/maps?q=${encodedQuery}&output=embed`,
    );
  }

  private createExternalMapUrl(searchQuery: string): string {
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(searchQuery)}`;
  }
}
