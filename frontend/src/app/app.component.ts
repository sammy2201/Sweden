import { Component, computed, inject, signal } from "@angular/core";
import { Router, RouterLink, RouterOutlet } from "@angular/router";
import { AuthService } from "./services/auth.service";
import { ButtonModule } from "primeng/button";
import { SiteFooterComponent } from "./components/site-footer/site-footer.component";
import { GuideDirectoryService } from "./services/guide-directory.service";
import type { GuideLink } from "./models/guide.models";

@Component({
  selector: "app-root",
  imports: [RouterOutlet, RouterLink, ButtonModule, SiteFooterComponent],
  templateUrl: "./app.component.html",
  styleUrl: "./app.component.css",
})
export class AppComponent {
  readonly authService = inject(AuthService);
  readonly router = inject(Router);
  private readonly guideDirectory = inject(GuideDirectoryService);
  readonly searchQuery = signal("");
  readonly searchFocused = signal(false);
  readonly mobileSearchOpen = signal(false);
  readonly mobileMenuOpen = signal(false);
  readonly activeSuggestionIndex = signal(0);
  readonly suggestions = computed(() =>
    this.guideDirectory.search(this.searchQuery()).slice(0, 6),
  );
  readonly searchPanelVisible = computed(
    () => this.searchFocused() || this.mobileSearchOpen(),
  );
  readonly activeSuggestionId = computed(() => {
    const index = this.activeSuggestionIndex();
    return index >= 0 && index < this.suggestions().length
      ? `header-guide-suggestion-${index}`
      : null;
  });

  private searchBlurTimer: number | undefined;

  onSearchInput(value: string): void {
    this.searchQuery.set(value);
    this.activeSuggestionIndex.set(0);
    this.searchFocused.set(true);
  }

  onSearchFocus(): void {
    if (this.searchBlurTimer !== undefined) {
      window.clearTimeout(this.searchBlurTimer);
      this.searchBlurTimer = undefined;
    }
    this.searchFocused.set(true);
  }

  onSearchBlur(): void {
    this.searchBlurTimer = window.setTimeout(() => {
      this.searchFocused.set(false);
      this.searchBlurTimer = undefined;
    }, 120);
  }

  onSearchKeydown(event: KeyboardEvent): void {
    const suggestionCount = this.suggestions().length;

    if (event.key === "Escape") {
      event.preventDefault();
      this.searchFocused.set(false);
      this.mobileSearchOpen.set(false);
      return;
    }

    if (suggestionCount === 0) {
      return;
    }

    if (event.key === "ArrowDown") {
      event.preventDefault();
      this.activeSuggestionIndex.update((index) =>
        index < 0 ? 0 : (index + 1) % suggestionCount,
      );
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      this.activeSuggestionIndex.update((index) =>
        index <= 0 ? suggestionCount - 1 : index - 1,
      );
    } else if (event.key === "Enter") {
      event.preventDefault();
      const activeIndex = this.activeSuggestionIndex();
      const guide = this.suggestions()[activeIndex < 0 ? 0 : activeIndex];
      if (guide) {
        this.navigateToGuide(guide);
      }
    }
  }

  selectSuggestion(guide: GuideLink): void {
    this.navigateToGuide(guide);
  }

  clearSearch(): void {
    this.searchQuery.set("");
    this.activeSuggestionIndex.set(0);
    this.searchFocused.set(true);
  }

  toggleMobileSearch(): void {
    this.mobileSearchOpen.update((open) => !open);
    this.mobileMenuOpen.set(false);
    this.searchFocused.set(false);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((open) => !open);
    this.mobileSearchOpen.set(false);
    this.searchFocused.set(false);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  navigateToGuide(guide: GuideLink): void {
    this.searchQuery.set("");
    this.searchFocused.set(false);
    this.mobileSearchOpen.set(false);
    this.mobileMenuOpen.set(false);
    this.router.navigateByUrl(guide.route);
  }

  logout(): void {
    this.mobileMenuOpen.set(false);
    this.mobileSearchOpen.set(false);
    this.searchFocused.set(false);
    this.authService.logout();
    this.router.navigate(["/login"]);
  }
}
