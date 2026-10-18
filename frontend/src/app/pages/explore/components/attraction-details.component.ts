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
  readonly currentUser = this.authService.user;

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

  planJourney(attraction: AttractionDetail, userCity?: string | null): void {
    const destination = attraction.city || attraction.name;
    const origin = userCity?.trim();

    if (origin) {
      this.navigateToJourney(origin, destination);
      return;
    }

    if (!this.authService.isAuthenticated()) {
      this.navigateToJourney(undefined, destination);
      return;
    }

    this.authService
      .loadUserProfile()
      .pipe(take(1))
      .subscribe({
        next: (profile) => {
          this.navigateToJourney(profile.city?.trim() || undefined, destination);
        },
        error: () => {
          this.navigateToJourney(undefined, destination);
        },
      });
  }

  private navigateToJourney(from: string | undefined, to: string): void {
    this.router.navigate(["/transport"], {
      queryParams: {
        ...(from ? { from } : {}),
        to,
      },
    });
  }
}
