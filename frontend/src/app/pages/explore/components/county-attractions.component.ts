import { CommonModule } from "@angular/common";
import { HttpClient, HttpParams } from "@angular/common/http";
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
import { RatingModule } from "primeng/rating";
import { TagModule } from "primeng/tag";
import {
  Attraction,
  County,
  PagedResponse,
} from "../../../models/explore.models";
import { FormsModule } from "@angular/forms";
import { buildExploreErrorMessage } from "../explore.utils";
import { environment } from "../../../../environments/environment";
import { AuthService } from "../../../services/auth.service";

interface CountyAttractionsVm {
  loading: boolean;
  error: string | null;
  county: County | undefined;
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

interface ExploreCountiesResponseDto {
  counties: County[];
}

interface ExploreAttractionsResponseDto {
  attractions: Attraction[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Component({
  selector: "app-county-attractions",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ButtonModule,
    CardModule,
    DividerModule,
    RatingModule,
    TagModule,
  ],
  templateUrl: "./county-attractions.component.html",
  styleUrl: "./county-attractions.component.css",
})
export class CountyAttractionsComponent {
  private readonly http: HttpClient = inject(HttpClient);
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
        this.getCounty(countyId),
        this.getAttractions(
          countyId,
          request.page,
          request.pageSize,
          request.category || undefined,
        ),
      ]).pipe(
        map(
          ([county, paged]): CountyAttractionsVm => ({
            loading: false,
            error: null,
            county,
            paged,
          }),
        ),
        catchError((error: unknown) =>
          of({
            loading: false,
            error: buildExploreErrorMessage(error),
            county: undefined,
            paged: EMPTY_PAGED_RESPONSE,
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

  ratingStars(rating: number): number {
    return Math.round(rating);
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

  private getCounty(countyId: string) {
    return this.http
      .get<ExploreCountiesResponseDto>(`${environment.apiUrl}/explore/counties`)
      .pipe(
        map((response) => response.counties ?? []),
        map((counties) => counties.find((county) => county.id === countyId)),
      );
  }

  private getAttractions(
    county: string,
    page: number,
    pageSize: number,
    category?: string,
  ) {
    let params = new HttpParams()
      .set("county", county)
      .set("page", page)
      .set("pageSize", pageSize);

    const cleanedCategory = category?.trim();
    if (cleanedCategory) {
      params = params.set("category", cleanedCategory);
    }

    return this.http
      .get<ExploreAttractionsResponseDto>(
        `${environment.apiUrl}/explore/attractions`,
        {
          params,
        },
      )
      .pipe(
        map(
          (response): PagedResponse<Attraction> => ({
            items: response.attractions ?? [],
            page: response.page,
            pageSize: response.pageSize,
            totalCount: response.totalCount,
            totalPages: response.totalPages,
            hasNextPage: response.hasNextPage,
            hasPreviousPage: response.hasPreviousPage,
          }),
        ),
      );
  }
}
