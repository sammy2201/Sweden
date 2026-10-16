import { Component, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";
import { GuideDirectoryService } from "../../services/guide-directory.service";

@Component({
  selector: "app-home",
  standalone: true,
  imports: [FormsModule, RouterLink, ButtonModule, CardModule, InputTextModule],
  templateUrl: "./home.component.html",
  styleUrl: "./home.component.css",
})
export class HomeComponent {
  private readonly guideDirectory = inject(GuideDirectoryService);
  readonly searchQuery = signal("");
  readonly essentials = this.guideDirectory.essentials;
  readonly isSearching = computed(() => this.searchQuery().trim().length > 0);
  readonly filteredGuides = computed(() => {
    const query = this.searchQuery().trim().toLocaleLowerCase();
    const matches = this.guideDirectory.search(query);
    return query ? matches : matches.filter((guide) => !guide.essential);
  });

  clearSearch(): void {
    this.searchQuery.set("");
  }
}
