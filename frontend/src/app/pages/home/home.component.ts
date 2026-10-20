import { Component, OnInit, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";
import { AuthService } from "../../services/auth.service";
import { ChatWidgetComponent } from "../../components/chat-widget/chat-widget.component";
import { GuideDirectoryService } from "../../services/guide-directory.service";
import { RoadmapService } from "../../services/roadmap.service";
import { getApiErrorStatus } from "../../utils/api-error";

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

  async ngOnInit(): Promise<void> {
    if (!this.authService.isAuthenticated()) {
      this.roadmapLookupComplete.set(true);
      return;
    }

    try {
      const roadmap = await this.roadmapService.getRoadmap();
      this.hasRoadmap.set(Boolean(roadmap?.id));
    } catch (error: unknown) {
      this.hasRoadmap.set(false);
      this.roadmapLookupFailed.set(getApiErrorStatus(error) !== 404);
    } finally {
      this.roadmapLookupComplete.set(true);
    }
  }

  clearSearch(): void {
    this.searchQuery.set("");
  }
}
