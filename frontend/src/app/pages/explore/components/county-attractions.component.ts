import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import {
  BehaviorSubject,
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
import { TagModule } from "primeng/tag";
import {
  Attraction,
  County,
  PagedResponse,
} from "../../../models/explore.models";
import { buildExploreErrorMessage } from "../explore.utils";
import { AuthService } from "../../../services/auth.service";
import { ExploreService } from "../../../services/explore.service";
import { getCountyImagePath } from "../county-images";

interface CountyAttractionsVm {
  loading: boolean;
  error: string | null;
  county: County | undefined;
  paged: PagedResponse<Attraction>;
}

interface AttractionsLoadState {
  error: string | null;
  paged: PagedResponse<Attraction>;
}

const EMPTY_PAGED_RESPONSE: PagedResponse<Attraction> = {
  items: [],
  page: 1,
  pageSize: 12,
  totalCount: 0,
  totalPages: 0,
  hasNextPage: false,
  hasPreviousPage: false,
};

@Component({
  selector: "app-county-attractions",
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ButtonModule,
    CardModule,
    DividerModule,
    TagModule,
  ],
  templateUrl: "./county-attractions.component.html",
  styleUrl: "./county-attractions.component.css",
})
export class CountyAttractionsComponent {
  private readonly exploreService = inject(ExploreService);
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly authService: AuthService = inject(AuthService);
  readonly currentUser = this.authService.user;

  countyImagePath(countyId: string): string {
    return getCountyImagePath(countyId);
  }
  private readonly requestState$ = new BehaviorSubject<{
    page: number;
    pageSize: number;
    category: string;
  }>({
    page: 1,
    pageSize: 12,
    category: "",
  });

  readonly vm$ = combineLatest([
    this.route.paramMap.pipe(map((params) => params.get("countyId") ?? "")),
    this.requestState$,
  ]).pipe(
    switchMap(([countyId, request]) =>
      combineLatest([
        this.exploreService.getCounty(countyId),
        this.exploreService.getAttractions(
          countyId,
          request.page,
          request.pageSize,
          request.category || undefined,
        ).pipe(
          map(
            (paged): AttractionsLoadState => ({
              error: null,
              paged,
            }),
          ),
          catchError((error: unknown) =>
            of<AttractionsLoadState>({
              error: buildExploreErrorMessage(error),
              paged: EMPTY_PAGED_RESPONSE,
            }),
          ),
        ),
      ]).pipe(
        map(
          ([county, attractions]): CountyAttractionsVm => ({
            loading: false,
            error: attractions.error,
            county,
            paged: attractions.paged,
          }),
        ),
      ),
    ),
    startWith({
      loading: true,
      error: null,
      county: undefined,
      paged: EMPTY_PAGED_RESPONSE,
    }),
  );

  retry(): void {
    this.requestState$.next({ ...this.requestState$.value });
  }

  goToPreviousPage(currentPage: number): void {
    if (currentPage <= 1) {
      return;
    }

    const current = this.requestState$.value;
    this.requestState$.next({
      ...current,
      page: currentPage - 1,
    });
  }

  goToNextPage(currentPage: number): void {
    const current = this.requestState$.value;
    this.requestState$.next({
      ...current,
      page: currentPage + 1,
    });
  }

  planJourney(attraction: Attraction, userCity?: string | null): void {
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
