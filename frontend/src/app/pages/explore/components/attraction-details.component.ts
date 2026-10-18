import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import {
  catchError,
  combineLatest,
  map,
  of,
  BehaviorSubject,
  startWith,
  switchMap,
  take,
} from "rxjs";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ImageModule } from "primeng/image";
import { PanelModule } from "primeng/panel";
import { TabsModule } from "primeng/tabs";
import { TagModule } from "primeng/tag";
import { AttractionDetail, County } from "../../../models/explore.models";
import { buildExploreErrorMessage } from "../explore.utils";
import { AuthService } from "../../../services/auth.service";
import { ExploreService } from "../../../services/explore.service";

interface AttractionDetailVm {
  loading: boolean;
  countyError: string | null;
  attractionError: string | null;
  county: County | undefined;
  attraction: AttractionDetail | undefined;
}

@Component({
  selector: "app-attraction-details",
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ButtonModule,
    CardModule,
    DividerModule,
    ImageModule,
    PanelModule,
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
  private readonly authService = inject(AuthService);
  private readonly retryRequest$ = new BehaviorSubject(0);

  readonly vm$ = combineLatest([this.route.paramMap, this.retryRequest$]).pipe(
    switchMap(([params]) => {
      const countyId = params.get("countyId") ?? "";
      const attractionId = params.get("attractionId") ?? "";

      return combineLatest({
        county: this.exploreService.getCounty(countyId).pipe(
          map((county) => ({ county, error: null as string | null })),
          catchError((error: unknown) => of({ county: undefined, error: buildExploreErrorMessage(error) })),
        ),
        attraction: this.exploreService.getAttraction(attractionId).pipe(
          map((attraction) => ({ attraction, error: null as string | null })),
          catchError((error: unknown) => of({ attraction: undefined, error: buildExploreErrorMessage(error) })),
        ),
      }).pipe(
        map(({ county, attraction }): AttractionDetailVm => ({
          loading: false,
          countyError: county.error,
          attractionError: attraction.error,
          county: county.county,
          attraction: attraction.attraction,
        })),
        startWith({
          loading: true,
          countyError: null,
          attractionError: null,
          county: undefined,
          attraction: undefined,
        }),
      );
    }),
  );

  retry(): void {
    this.retryRequest$.next(this.retryRequest$.value + 1);
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
}
