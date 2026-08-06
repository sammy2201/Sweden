import { CommonModule } from "@angular/common";
import { Component, OnInit, computed, inject, signal } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { InputTextModule } from "primeng/inputtext";
import { SkeletonModule } from "primeng/skeleton";
import { TagModule } from "primeng/tag";
import { AuthService, UserProfile } from "../../services/auth.service";

@Component({
  selector: "app-profile",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    CardModule,
    DividerModule,
    InputTextModule,
    SkeletonModule,
    TagModule,
  ],
  templateUrl: "./profile.component.html",
  styleUrl: "./profile.component.css",
})
export class ProfileComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly profile = signal<UserProfile | null>(null);
  readonly loading = signal(true);
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal("");
  readonly saveMessage = signal("");

  readonly form = this.fb.nonNullable.group({
    firstName: ["", [Validators.required, Validators.minLength(2)]],
    lastName: ["", [Validators.required, Validators.minLength(2)]],
    city: ["", [Validators.required, Validators.minLength(2)]],
    address: ["", [Validators.required, Validators.minLength(3)]],
  });

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
      this.setProfile(cachedProfile);
      this.loading.set(false);
      return;
    }

    this.authService.loadUserProfile().subscribe({
      next: (profile) => {
        this.setProfile(profile);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set("We could not load your profile right now.");
        this.loading.set(false);
      },
    });
  }

  startEditing(): void {
    const profile = this.profile();
    if (!profile) {
      return;
    }

    this.form.patchValue({
      firstName: profile.firstName,
      lastName: profile.lastName,
      city: profile.city,
      address: profile.address,
    });
    this.errorMessage.set("");
    this.saveMessage.set("");
    this.isEditing.set(true);
  }

  cancelEditing(): void {
    const profile = this.profile();
    if (profile) {
      this.form.patchValue({
        firstName: profile.firstName,
        lastName: profile.lastName,
        city: profile.city,
        address: profile.address,
      });
    }

    this.errorMessage.set("");
    this.saveMessage.set("");
    this.isEditing.set(false);
  }

  saveProfile(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set("");
    this.saveMessage.set("");

    this.authService.updateUserProfile(this.form.getRawValue()).subscribe({
      next: (profile) => {
        this.setProfile(profile);
        this.isEditing.set(false);
        this.isSaving.set(false);
        this.saveMessage.set("Profile updated successfully.");
      },
      error: () => {
        this.errorMessage.set("We could not save your profile changes.");
        this.isSaving.set(false);
      },
    });
  }

  isInvalid(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.dirty || control.touched);
  }

  private setProfile(profile: UserProfile): void {
    this.profile.set(profile);
    this.form.patchValue({
      firstName: profile.firstName,
      lastName: profile.lastName,
      city: profile.city,
      address: profile.address,
    });
  }
}
