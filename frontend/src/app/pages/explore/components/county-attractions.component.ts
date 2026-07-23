import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { combineLatest, map, switchMap } from "rxjs";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ChipModule } from "primeng/chip";
import { DividerModule } from "primeng/divider";
import { RatingModule } from "primeng/rating";
import { TagModule } from "primeng/tag";
import { Attraction, County } from "../../../models/explore.models";
import { ExploreService } from "../../../services/explore.service";
import { FormsModule } from "@angular/forms";

@Component({
  selector: "app-county-attractions",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ButtonModule,
    CardModule,
    ChipModule,
    DividerModule,
    RatingModule,
    TagModule,
  ],
  templateUrl: "./county-attractions.component.html",
  styleUrl: "./county-attractions.component.css",
})
export class CountyAttractionsComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly exploreService = inject(ExploreService);

  readonly vm$ = this.route.paramMap.pipe(
    switchMap((params) => {
      const countyId = params.get("countyId") ?? "";

      return combineLatest([
        this.exploreService.getCounty(countyId),
        this.exploreService.getAttractionsByCounty(countyId),
      ]);
    }),
    map(([county, attractions]) => ({ county, attractions })),
  );

  ratingStars(rating: number): number {
    return Math.round(rating);
  }

  planJourney(county: County, attraction: Attraction): void {
    this.router.navigate(["/transport"], {
      queryParams: {
        from: county.defaultOrigin,
        to: attraction.nearestStation || attraction.name,
      },
    });
  }
}
