import { Component, input } from "@angular/core";
import type { Bank } from "../bank.model";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { CarouselModule } from "primeng/carousel";

@Component({
  selector: "app-banking-banks",
  standalone: true,
  imports: [ButtonModule, CardModule, TableModule, TagModule, CarouselModule],
  templateUrl: "./banking-banks.component.html",
})
export class BankingBanksComponent {
  banks = input.required<Bank[]>();

  trackByBankName = (_: number, bank: Bank): string => bank.name;

  openWebsite(url: string): void {
    globalThis.open(url, "_blank", "noopener,noreferrer");
  }
  responsiveOptions = [
    {
      breakpoint: "1200px",
      numVisible: 3,
      numScroll: 1,
    },
    {
      breakpoint: "992px",
      numVisible: 2,
      numScroll: 1,
    },
    {
      breakpoint: "768px",
      numVisible: 1,
      numScroll: 1,
    },
  ];
}
