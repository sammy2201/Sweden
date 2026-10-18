import { CommonModule } from "@angular/common";
import {
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Router } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { InputTextModule } from "primeng/inputtext";
import { ProgressBarModule } from "primeng/progressbar";
import { SkeletonModule } from "primeng/skeleton";
import { TagModule } from "primeng/tag";
import { AuthService, UserProfile } from "../../services/auth.service";
import {
  RoadmapResponse,
  RoadmapTaskResponse,
  RoadmapService,
} from "../../services/roadmap.service";
import { getApiErrorMessage } from "../../utils/api-error";
import { HttpErrorResponse } from "@angular/common/http";

@Component({
  selector: "app-profile",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    DividerModule,
    InputTextModule,
    ProgressBarModule,
    SkeletonModule,
    TagModule,
  ],
  templateUrl: "./profile.component.html",
  styleUrl: "./profile.component.css",
})
export class ProfileComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly roadmapService = inject(RoadmapService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  readonly profile = signal<UserProfile | null>(null);
  readonly loading = signal(true);
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal("");
  readonly saveMessage = signal("");
  readonly roadmap = signal<RoadmapResponse | null>(null);
  readonly roadmapLoading = signal(true);
  readonly roadmapError = signal("");

  readonly applicableRoadmapTasks = computed(
    () => this.roadmap()?.tasks.filter((task) =>
      task.state !== "NotApplicable" && !this.isOptionalRoadmapTask(task),
    ) ?? [],
  );
  readonly completedRoadmapTasks = computed(
    () => this.applicableRoadmapTasks().filter((task) => task.completed || task.state === "Completed").length,
  );
  readonly roadmapProgress = computed(() => {
    const tasks = this.applicableRoadmapTasks();
    return tasks.length === 0
      ? 0
      : Math.round((this.completedRoadmapTasks() / tasks.length) * 100);
  });

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
    this.loadRoadmap();
    const cachedProfile = this.authService.user();

    if (cachedProfile) {
      this.setProfile(cachedProfile);
      this.loading.set(false);
      return;
    }

    this.authService
      .loadUserProfile()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (profile) => {
          this.setProfile(profile);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            getApiErrorMessage(
              error,
              "We could not load your profile right now.",
            ),
          );
          this.loading.set(false);
        },
      });
  }

  private loadRoadmap(): void {
    this.roadmapService
      .getRoadmap()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (roadmap) => {
          this.roadmap.set(roadmap);
          this.roadmapLoading.set(false);
        },
        error: (error: unknown) => {
          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.roadmap.set(null);
          } else {
            this.roadmapError.set(
              getApiErrorMessage(error, "We could not load your roadmap."),
            );
          }
          this.roadmapLoading.set(false);
        },
      });
  }

  openRoadmap(): void {
    this.router.navigateByUrl("/roadmap");
  }

  isOptionalRoadmapTask(task: RoadmapTaskResponse): boolean {
    return task.state === "Optional" ||
      (task.state === "Completed" && task.stateBeforeCompletion === "Optional");
  }

  roadmapTaskStatusLabel(task: RoadmapTaskResponse): string {
    if (task.state === "NotApplicable") return "Not applicable";
    if (task.completed || task.state === "Completed") return "Completed";
    return this.isOptionalRoadmapTask(task) ? "Optional" : task.state;
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

    this.authService
      .updateUserProfile({
        ...this.form.getRawValue(),
        transportCountyId: this.profile()?.transportCountyId ?? null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (profile) => {
          this.setProfile(profile);
          this.isEditing.set(false);
          this.isSaving.set(false);
          this.saveMessage.set("Profile updated successfully.");
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            getApiErrorMessage(
              error,
              "We could not save your profile changes.",
            ),
          );
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
