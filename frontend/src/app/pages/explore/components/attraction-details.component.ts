import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { combineLatest, map, switchMap } from "rxjs";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ChipModule } from "primeng/chip";
import { DividerModule } from "primeng/divider";
import { ImageModule } from "primeng/image";
import { PanelModule } from "primeng/panel";
import { RatingModule } from "primeng/rating";
import { TabsModule } from "primeng/tabs";
import { TagModule } from "primeng/tag";
import { Attraction, County } from "../../../models/explore.models";
import { ExploreService } from "../../../services/explore.service";

@Component({
  selector: "app-attraction-details",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ButtonModule,
    CardModule,
    ChipModule,
    DividerModule,
    ImageModule,
    PanelModule,
    RatingModule,
    TabsModule,
    TagModule,
  ],
  templateUrl: "./attraction-details.component.html",
  styleUrl: "./attraction-details.component.css",
})
export class AttractionDetailsComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly exploreService = inject(ExploreService);

  readonly vm$ = this.route.paramMap.pipe(
    switchMap((params) => {
      const countyId = params.get("countyId") ?? "";
      const attractionId = params.get("attractionId") ?? "";

      return combineLatest([
        this.exploreService.getCounty(countyId),
        this.exploreService.getAttraction(countyId, attractionId),
      ]);
    }),
    map(([county, attraction]) => ({ county, attraction })),
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
