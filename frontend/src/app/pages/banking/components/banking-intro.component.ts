import { Component, input } from "@angular/core";
import { AvatarModule } from "primeng/avatar";
import { CardModule } from "primeng/card";
import { ProgressBarModule } from "primeng/progressbar";
import { TagModule } from "primeng/tag";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "app-banking-intro",
  standalone: true,
  imports: [
    AvatarModule,
    CardModule,
    ProgressBarModule,
    TagModule,
    ButtonModule,
  ],
  templateUrl: "./banking-intro.component.html",
})
export class BankingIntroComponent {
  userName = input.required<string>();
  progress = input.required<number>();

  openWebsite(url: string): void {
    window.open(url, "_blank", "noopener,noreferrer");
  }
}
