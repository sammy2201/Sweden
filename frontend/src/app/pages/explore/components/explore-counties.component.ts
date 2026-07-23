import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";
import { ExploreService } from "../../../services/explore.service";

@Component({
  selector: "app-explore-counties",
  standalone: true,
  imports: [CommonModule, RouterLink, CardModule, ButtonModule, TagModule],
  templateUrl: "./explore-counties.component.html",
  styleUrl: "./explore-counties.component.css",
})
export class ExploreCountiesComponent {
  private readonly exploreService = inject(ExploreService);

  readonly counties$ = this.exploreService.getCounties();
}
