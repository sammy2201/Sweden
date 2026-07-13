import { Component } from "@angular/core";

interface TransportProvider {
  name: string;
  icon: string;
  description: string;
  website: string;
  ticketInfo: string[];
  color: string;
}

@Component({
  selector: "app-transport",
  standalone: true,
  templateUrl: "./transport.component.html",
  styleUrls: ["./transport.component.css"],
})
export class TransportComponent {
  providers: TransportProvider[] = [
    {
      name: "County Public Transport",
      icon: "pi pi-map",
      color: "#2563eb",
      website: "https://www.skanetrafiken.se",
      description:
        "Local buses, regional trains and city transport operated by your county.",
      ticketInfo: [
        "Download your county transport app (e.g. Skånetrafiken, SL, Västtrafik).",
        "Create an account.",
        "Search your journey.",
        "Buy a single or period ticket.",
        "Show the QR code when requested.",
      ],
    },
    {
      name: "SJ",
      icon: "pi pi-send",
      color: "#111827",
      website: "https://www.sj.se",
      description: "Sweden's largest train operator for travel between cities.",
      ticketInfo: [
        "Download the SJ app.",
        "Search departure and destination.",
        "Select a train.",
        "Pay using card or Swish.",
        "Ticket appears in the app.",
      ],
    },
    {
      name: "Snälltåget",
      icon: "pi pi-car",
      color: "#15803d",
      website: "https://www.snalltaget.se",
      description: "Affordable long-distance trains connecting major cities.",
      ticketInfo: [
        "Book on the website or app.",
        "Choose seat.",
        "Pay securely.",
        "Receive your ticket by email or app.",
      ],
    },
    {
      name: "FlixBus",
      icon: "pi pi-directions",
      color: "#22c55e",
      website: "https://www.flixbus.com",
      description: "Budget coach travel within Sweden and across Europe.",
      ticketInfo: [
        "Download FlixBus.",
        "Search your route.",
        "Choose departure.",
        "Pay online.",
        "Board using your QR ticket.",
      ],
    },
  ];
}
