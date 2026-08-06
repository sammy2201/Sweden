import { CommonModule } from "@angular/common";
import { Component, OnInit, computed, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { TagModule } from "primeng/tag";
import { AuthService, UserProfile } from "../../services/auth.service";

@Component({
  selector: "app-profile",
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ButtonModule,
    CardModule,
    DividerModule,
    SkeletonModule,
    TagModule,
  ],
  templateUrl: "./profile.component.html",
  styleUrl: "./profile.component.css",
})
export class ProfileComponent implements OnInit {
  private readonly authService = inject(AuthService);

  readonly profile = signal<UserProfile | null>(null);
  readonly loading = signal(true);
  readonly errorMessage = signal("");

  readonly initials = computed(() => {
    const profile = this.profile();

    if (!profile) {
      return "SS";
    }

    return `${profile.firstName.charAt(0)}${profile.lastName.charAt(0)}`
      .trim()
      .toUpperCase();
  });

  ngOnInit(): void {
    const cachedProfile = this.authService.user();

    if (cachedProfile) {
      this.profile.set(cachedProfile);
      this.loading.set(false);
      return;
    }

    this.authService.loadUserProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set("We could not load your profile right now.");
        this.loading.set(false);
      },
    });
  }
}
