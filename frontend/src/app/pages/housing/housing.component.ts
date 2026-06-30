import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { AccordionModule } from "primeng/accordion";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { TagModule } from "primeng/tag";

@Component({
  selector: "app-housing",
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    TagModule,
    AccordionModule,
    ButtonModule,
    DividerModule,
  ],
  templateUrl: "./housing.component.html",
  styleUrl: "./housing.component.css",
})
export class HousingComponent {
  housingSteps = [
    {
      step: "01",
      title: "Housing Queues",
      description:
        "Many apartments are rented through housing queues. The earlier you register, the better your chances of receiving an apartment offer.",
    },
    {
      step: "02",
      title: "Private Rentals",
      description:
        "Private landlords and rental platforms offer first-hand and second-hand apartments with varying requirements and availability.",
    },
    {
      step: "03",
      title: "Stay Safe",
      description:
        "Rental scams exist. Never pay deposits before verifying the landlord and signing a proper rental contract.",
    },
  ];

  platforms = [
    {
      name: "HomeQ",
      description:
        "One of Sweden's largest housing queues. Many private landlords list apartments here. Register early to begin collecting queue days.",
    },
    {
      name: "Boplats",
      description:
        "Official housing queue used by several municipalities including Malmö and Gothenburg. Excellent for long-term rentals.",
    },
    {
      name: "Qasa",
      description:
        "A popular marketplace for second-hand rentals with verified users, secure payments and digital rental agreements.",
    },
    {
      name: "Blocket Bostad",
      description:
        "One of Sweden's most popular rental marketplaces. Always verify the landlord before making any payment.",
    },
  ];

  tips = [
    "Register for housing queues as early as possible.",
    "Search every day because new listings appear frequently.",
    "Prepare employment or study documents in advance.",
    "Keep your profile complete on rental platforms.",
    "Respond quickly when apartments become available.",
    "Read the rental agreement carefully before signing.",
  ];

  faqs = [
    {
      value: 0,
      question: "Never pay before viewing",
      answer:
        "Do not send deposits or rent before viewing the apartment or confirming the landlord's identity.",
    },
    {
      value: 1,
      question: "Ask for a rental agreement",
      answer:
        "Always request a written rental contract that clearly states the rent, deposit, move-in date and rental period.",
    },
    {
      value: 2,
      question: "Watch for warning signs",
      answer:
        "Be cautious if the rent is unusually cheap, the landlord refuses to meet, or asks for payment through unusual methods.",
    },
  ];

  resources = [
    { label: "HomeQ", url: "https://www.homeq.se" },
    { label: "Qasa", url: "https://www.qasa.se" },
    { label: "Blocket Bostad", url: "https://bostad.blocket.se" },
    { label: "Boplats Syd", url: "https://www.boplatsyd.se" },
    {
      label: "Bostadsförmedlingen Stockholm",
      url: "https://bostad.stockholm.se",
    },
  ];
}
