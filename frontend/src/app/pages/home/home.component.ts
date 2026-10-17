import {
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from "@angular/core";
import { HttpErrorResponse } from "@angular/common/http";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormsModule } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";
import { AuthService } from "../../services/auth.service";
import { ChatWidgetComponent } from "../../components/chat-widget/chat-widget.component";
import { GuideDirectoryService } from "../../services/guide-directory.service";
import { RoadmapService } from "../../services/roadmap.service";

@Component({
  selector: "app-home",
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    ButtonModule,
    CardModule,
    InputTextModule,
    ChatWidgetComponent,
  ],
  templateUrl: "./home.component.html",
  styleUrl: "./home.component.css",
})
export class HomeComponent implements OnInit {
  private readonly guideDirectory = inject(GuideDirectoryService);
  private readonly authService = inject(AuthService);
  private readonly roadmapService = inject(RoadmapService);
  private readonly destroyRef = inject(DestroyRef);
  readonly searchQuery = signal("");
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly hasRoadmap = signal(false);
  readonly roadmapLookupComplete = signal(false);
  readonly roadmapLookupFailed = signal(false);
  readonly essentials = this.guideDirectory.essentials;
  readonly isSearching = computed(() => this.searchQuery().trim().length > 0);
  readonly filteredGuides = computed(() => {
    const query = this.searchQuery().trim().toLocaleLowerCase();
    const matches = this.guideDirectory.search(query);
    return query ? matches : matches.filter((guide) => !guide.essential);
  });

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.roadmapLookupComplete.set(true);
      return;
    }

    this.roadmapService
      .getRoadmap()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (roadmap) => {
          this.hasRoadmap.set(Boolean(roadmap?.id));
          this.roadmapLookupComplete.set(true);
        },
        error: (error: unknown) => {
          const roadmapIsMissing =
            error instanceof HttpErrorResponse && error.status === 404;
          this.hasRoadmap.set(false);
          this.roadmapLookupFailed.set(!roadmapIsMissing);
          this.roadmapLookupComplete.set(true);
        },
      });
  }

  clearSearch(): void {
    this.searchQuery.set("");
  }
}
