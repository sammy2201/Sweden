import { CommonModule } from "@angular/common";
import { Component, DestroyRef, OnInit, inject } from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
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
export class CountyAttractionsComponent implements OnInit {
  private readonly exploreService = inject(ExploreService);
  private readonly route: ActivatedRoute = inject(ActivatedRoute);
  private readonly router: Router = inject(Router);
  private readonly authService: AuthService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private countyId = "";
  private request = { page: 1, pageSize: 12, category: "" };
  private loadSequence = 0;
  readonly currentUser = this.authService.user;
  vm: CountyAttractionsVm = {
    loading: true,
    error: null,
    county: undefined,
    paged: EMPTY_PAGED_RESPONSE,
  };

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        this.countyId = params.get("countyId") ?? "";
        void this.loadAttractions();
      });
  }

  countyImagePath(countyId: string): string {
    return getCountyImagePath(countyId);
  }
  private async loadAttractions(): Promise<void> {
    const sequence = ++this.loadSequence;
    this.vm = { ...this.vm, loading: true, error: null };

    const [countyResult, attractionsResult] = await Promise.allSettled([
      this.exploreService.getCounty(this.countyId),
      this.exploreService.getAttractions(
        this.countyId,
        this.request.page,
        this.request.pageSize,
        this.request.category || undefined,
      ),
    ]);
    if (sequence !== this.loadSequence) return;

    this.vm = {
      loading: false,
      error:
        attractionsResult.status === "rejected"
          ? buildExploreErrorMessage(attractionsResult.reason)
          : countyResult.status === "rejected"
            ? buildExploreErrorMessage(countyResult.reason)
            : null,
      county:
        countyResult.status === "fulfilled" ? countyResult.value : undefined,
      paged:
        attractionsResult.status === "fulfilled"
          ? attractionsResult.value
          : EMPTY_PAGED_RESPONSE,
    };
  }

  retry(): void {
    void this.loadAttractions();
  }

  goToPreviousPage(currentPage: number): void {
    if (currentPage <= 1) {
      return;
    }

    this.request.page = currentPage - 1;
    void this.loadAttractions();
  }

  goToNextPage(currentPage: number): void {
    this.request.page = currentPage + 1;
    void this.loadAttractions();
  }

  async planJourney(
    attraction: Attraction,
    userCity?: string | null,
  ): Promise<void> {
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

    try {
      const profile = await this.authService.loadUserProfile();
      this.navigateToJourney(profile.city?.trim() || undefined, destination);
    } catch {
      this.navigateToJourney(undefined, destination);
    }
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
