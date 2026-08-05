import { CommonModule } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Component, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import {
  catchError,
  combineLatest,
  map,
  of,
  startWith,
  switchMap,
  take,
} from "rxjs";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ImageModule } from "primeng/image";
import { PanelModule } from "primeng/panel";
import { RatingModule } from "primeng/rating";
import { TabsModule } from "primeng/tabs";
import { TagModule } from "primeng/tag";
import { AttractionDetail, County } from "../../../models/explore.models";
import { buildExploreErrorMessage } from "../explore.utils";
import { AuthService } from "../../../services/auth.service";
import { environment } from "../../../../environments/environment";

interface ExploreCountiesResponseDto {
  counties: County[];
}

interface ExploreAttractionDetailResponseDto {
  attraction: AttractionDetail;
}

interface AttractionDetailVm {
  loading: boolean;
  error: string | null;
  county: County | undefined;
  attraction: AttractionDetail | undefined;
}

@Component({
  selector: "app-attraction-details",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ButtonModule,
    CardModule,
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
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);

  readonly vm$ = this.route.paramMap.pipe(
    switchMap((params) => {
      const countyId = params.get("countyId") ?? "";
      const attractionId = params.get("attractionId") ?? "";

      return combineLatest([
        this.getCounty(countyId),
        this.getAttraction(attractionId),
      ]);
    }),
    map(
      ([county, attraction]): AttractionDetailVm => ({
        loading: false,
        error: null,
        county,
        attraction,
      }),
    ),
    startWith({
      loading: true,
      error: null,
      county: undefined,
      attraction: undefined,
    }),
    catchError((error: unknown) =>
      of({
        loading: false,
        error: buildExploreErrorMessage(error),
        county: undefined,
        attraction: undefined,
      }),
    ),
  );

  ratingStars(rating: number): number {
    return Math.round(rating);
  }

  mapsUrl(attraction: AttractionDetail): string {
    const query = attraction.address || attraction.name;
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
  }

  planJourney(county: County | undefined, attraction: AttractionDetail): void {
    const fallbackOrigin = county?.name || "Stockholm";
    const destination = attraction.city || attraction.name;

    const cachedAddress = this.authService.user()?.address?.trim();
    if (cachedAddress) {
      this.navigateToJourney(cachedAddress, destination);
      return;
    }

    this.authService
      .loadUserProfile()
      .pipe(take(1))
      .subscribe({
        next: (profile) => {
          const origin = profile.address?.trim() || fallbackOrigin;
          this.navigateToJourney(origin, destination);
        },
        error: () => {
          this.navigateToJourney(fallbackOrigin, destination);
        },
      });
  }

  private navigateToJourney(from: string, to: string): void {
    this.router.navigate(["/transport"], {
      queryParams: {
        from,
        to,
      },
    });
  }

  private getCounty(countyId: string) {
    return this.http
      .get<ExploreCountiesResponseDto>(`${environment.apiUrl}/explore/counties`)
      .pipe(
        map((response) => response.counties ?? []),
        map((counties) => counties.find((county) => county.id === countyId)),
      );
  }

  private getAttraction(attractionId: string) {
    return this.http
      .get<ExploreAttractionDetailResponseDto>(
        `${environment.apiUrl}/explore/attractions/${encodeURIComponent(attractionId)}`,
      )
      .pipe(map((response) => response.attraction));
  }
}
