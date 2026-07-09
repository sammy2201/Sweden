import { Component } from "@angular/core";
import { TabsModule } from "primeng/tabs";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";

@Component({
  selector: "app-digital-id",
  standalone: true,
  imports: [TabsModule, ButtonModule, CardModule, TagModule],
  templateUrl: "./digital-ID.component.html",
  styleUrl: "./digital-ID.component.css",
})
export class DigitalIDComponent {
  bankIdFeatures = [
    "Log in to government services",
    "Approve bank transactions",
    "Use Swish",
    "Sign documents digitally",
    "Access healthcare (1177)",
  ];

  frejaFeatures = [
    "Government approved eID",
    "No Swedish bank required",
    "Works with many authorities",
    "Available on Android and iPhone",
  ];
}
