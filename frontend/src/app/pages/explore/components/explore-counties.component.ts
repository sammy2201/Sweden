import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { catchError, map, of, startWith } from "rxjs";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { SkeletonModule } from "primeng/skeleton";
import { TagModule } from "primeng/tag";
import { County } from "../../../models/explore.models";
import { ExploreService } from "../../../services/explore.service";
import { buildExploreErrorMessage } from "../explore.utils";

interface CountiesVm {
  loading: boolean;
  error: string | null;
  counties: County[];
}

@Component({
  selector: "app-explore-counties",
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    CardModule,
    ButtonModule,
    SkeletonModule,
    TagModule,
  ],
  templateUrl: "./explore-counties.component.html",
  styleUrl: "./explore-counties.component.css",
})
export class ExploreCountiesComponent {
  private readonly exploreService = inject(ExploreService);

  readonly vm$ = this.exploreService.getCounties().pipe(
    map(
      (counties): CountiesVm => ({
        loading: false,
        error: null,
        counties,
      }),
    ),
    startWith({
      loading: true,
      error: null,
      counties: [] as County[],
    }),
    catchError((error: unknown) =>
      of({
        loading: false,
        error: buildExploreErrorMessage(error),
        counties: [] as County[],
      }),
    ),
  );
}
