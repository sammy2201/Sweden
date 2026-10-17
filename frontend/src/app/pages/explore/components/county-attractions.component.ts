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

  planJourney(county: County | undefined, attraction: Attraction): void {
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
