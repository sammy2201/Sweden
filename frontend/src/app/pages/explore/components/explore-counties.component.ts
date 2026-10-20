import { CommonModule } from "@angular/common";
import { Component, OnInit, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { SkeletonModule } from "primeng/skeleton";
import { TagModule } from "primeng/tag";
import { County } from "../../../models/explore.models";
import { ExploreService } from "../../../services/explore.service";
import { buildExploreErrorMessage } from "../explore.utils";
import { getCountyImagePath } from "../county-images";

@Component({
  selector: "app-explore-counties",
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CardModule,
    ButtonModule,
    SkeletonModule,
    TagModule,
  ],
  templateUrl: "./explore-counties.component.html",
  styleUrl: "./explore-counties.component.css",
})
export class ExploreCountiesComponent implements OnInit {
  private readonly exploreService = inject(ExploreService);
  loading = true;
  error: string | null = null;
  counties: County[] = [];

  ngOnInit(): void {
    void this.loadCounties();
  }

  private async loadCounties(): Promise<void> {
    try {
      this.counties = await this.exploreService.getCounties();
    } catch (error: unknown) {
      this.error = buildExploreErrorMessage(error);
    } finally {
      this.loading = false;
    }
  }

  countyImagePath(countyId: string): string {
    return getCountyImagePath(countyId);
  }
}
