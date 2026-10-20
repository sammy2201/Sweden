import { CommonModule } from "@angular/common";
import { Component, DestroyRef, OnInit, inject } from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
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
export class AttractionDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly exploreService = inject(ExploreService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private countyId = "";
  private attractionId = "";
  private loadSequence = 0;
  readonly currentUser = this.authService.user;

  vm: AttractionDetailVm = {
    loading: true,
    countyError: null,
    attractionError: null,
    county: undefined,
    attraction: undefined,
  };

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        this.countyId = params.get("countyId") ?? "";
        this.attractionId = params.get("attractionId") ?? "";
        void this.loadDetails();
      });
  }

  private async loadDetails(): Promise<void> {
    const sequence = ++this.loadSequence;
    this.vm = { ...this.vm, loading: true };
    const [countyResult, attractionResult] = await Promise.allSettled([
      this.exploreService.getCounty(this.countyId),
      this.exploreService.getAttraction(this.attractionId),
    ]);
    if (sequence !== this.loadSequence) return;

    this.vm = {
      loading: false,
      countyError:
        countyResult.status === "rejected"
          ? buildExploreErrorMessage(countyResult.reason)
          : null,
      attractionError:
        attractionResult.status === "rejected"
          ? buildExploreErrorMessage(attractionResult.reason)
          : null,
      county:
        countyResult.status === "fulfilled" ? countyResult.value : undefined,
      attraction:
        attractionResult.status === "fulfilled"
          ? attractionResult.value
          : undefined,
    };
  }

  retry(): void {
    void this.loadDetails();
  }

  mapsUrl(attraction: AttractionDetail): string {
    const query = attraction.address || attraction.name;
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
  }

  async planJourney(
    attraction: AttractionDetail,
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
