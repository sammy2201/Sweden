import { CommonModule } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { Component, DestroyRef, OnInit, inject } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormsModule } from "@angular/forms";
import { ConfirmationService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CheckboxModule } from "primeng/checkbox";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { DialogModule } from "primeng/dialog";
import { ProgressBarModule } from "primeng/progressbar";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import {
  RoadmapRequest,
  RoadmapResponse,
  RoadmapService,
  RoadmapTaskResponse,
} from "../../services/roadmap.service";
import { getApiErrorMessage } from "../../utils/api-error";

interface RoadmapField {
  id: keyof RoadmapRequest;
  label: string;
  options: string[];
}

interface RoadmapValue {
  label: string;
  value: string;
}

interface Task {
  id: string;
  title: string;
  description: string;
  completed: boolean;
}

@Component({
  selector: "app-roadmap",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CheckboxModule,
    ConfirmDialogModule,
    DialogModule,
    ProgressBarModule,
    SelectModule,
    TableModule,
  ],
  providers: [ConfirmationService],
  templateUrl: "./roadmap.component.html",
  styleUrls: ["./roadmap.component.css"],
})
export class RoadmapComponent implements OnInit {
  private readonly roadmapService = inject(RoadmapService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly confirmationService = inject(ConfirmationService);

  readonly fields: RoadmapField[] = [
    { id: "origin", label: "Origin", options: ["EU", "Non-EU"] },
    {
      id: "residencePermit",
      label: "Residence permit",
      options: ["Yes", "No"],
    },
    { id: "liveInSweden", label: "Living in Sweden", options: ["Yes", "No"] },
    { id: "personnummer", label: "Personnummer", options: ["Yes", "No"] },
    {
      id: "appliedPersonnummer",
      label: "Applied for Personnummer",
      options: ["Yes", "No", "Waiting for decision", "Not applicable"],
    },
    { id: "idCard", label: "Swedish ID card", options: ["Yes", "No"] },
    {
      id: "bankAccount",
      label: "Swedish bank account",
      options: ["Yes", "No"],
    },
    { id: "bankId", label: "BankID", options: ["Yes", "No"] },
    {
      id: "housing",
      label: "Housing",
      options: ["Permanent", "Temporary", "Still looking"],
    },
    { id: "planToDrive", label: "Plans to drive", options: ["Yes", "No"] },
    {
      id: "drivingLicenceType",
      label: "Driving licence",
      options: ["Swedish", "EU", "Other", "Not applicable"],
    },
    {
      id: "purpose",
      label: "Purpose",
      options: ["Work", "Study", "Looking for work", "Family"],
    },
    {
      id: "insurance",
      label: "Registered with Försäkringskassan",
      options: ["Yes", "No"],
    },
  ];

  roadmapValues: RoadmapValue[] = [];
  editAnswers: Partial<Record<keyof RoadmapRequest, string>> = {};
  tasks: Task[] = [];
  hasExistingRoadmap = false;
  isLoadingExistingRoadmap = true;
  isEditingExistingRoadmap = false;
  isCreatingRoadmap = false;
  isGenerating = false;
  isDeleting = false;
  pendingTaskIds = new Set<string>();
  generationError = "";
  successMessage = "";

  get completedTaskCount(): number {
    return this.tasks.filter((task) => task.completed).length;
  }

  get taskProgressPercent(): number {
    if (this.tasks.length === 0) return 0;
    return Math.round((this.completedTaskCount / this.tasks.length) * 100);
  }

  get isRoadmapDialogVisible(): boolean {
    return this.isEditingExistingRoadmap || this.isCreatingRoadmap;
  }

  set isRoadmapDialogVisible(visible: boolean) {
    if (!visible) {
      this.cancelRoadmapUpdate();
    }
  }

  ngOnInit(): void {
    this.loadExistingRoadmap();
  }

  startRoadmapUpdate(): void {
    this.editAnswers = {};
    for (const field of this.fields) {
      const item = this.roadmapValues.find(
        (value) => value.label === field.label,
      );
      this.editAnswers[field.id] = item?.value ?? "";
    }
    this.generationError = "";
    this.successMessage = "";
    this.isEditingExistingRoadmap = true;
    this.isCreatingRoadmap = false;
  }

  startRoadmapCreation(): void {
    this.editAnswers = {};
    for (const field of this.fields) {
      this.editAnswers[field.id] = "";
    }
    this.generationError = "";
    this.successMessage = "";
    this.isCreatingRoadmap = true;
  }

  cancelRoadmapUpdate(): void {
    this.isEditingExistingRoadmap = false;
    this.isCreatingRoadmap = false;
    this.generationError = "";
  }

  saveRoadmap(): void {
    if (this.isGenerating) return;

    this.isGenerating = true;
    this.generationError = "";
    this.successMessage = "";
    const saveRequest = this.hasExistingRoadmap
      ? this.roadmapService.updateRoadmap(this.toRequest())
      : this.roadmapService.generateRoadmap(this.toRequest());
    saveRequest.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (response) => {
        this.setRoadmapValues(response);
        this.tasks = this.mapTasks(response.tasks ?? []);
        this.hasExistingRoadmap = true;
        this.isEditingExistingRoadmap = false;
        this.isCreatingRoadmap = false;
        this.isGenerating = false;
        this.successMessage = "Your roadmap has been saved.";
      },
      error: (error: unknown) => {
        this.generationError = getApiErrorMessage(
          error,
          "We could not save your roadmap right now. Please try again.",
        );
        this.isGenerating = false;
      },
    });
  }

  private loadExistingRoadmap(): void {
    this.roadmapService
      .getRoadmap()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          if (response?.id) {
            this.setRoadmapValues(response);
            this.tasks = this.mapTasks(response.tasks ?? []);
            this.hasExistingRoadmap = true;
          }
          this.isLoadingExistingRoadmap = false;
        },
        error: (error: unknown) => {
          this.generationError =
            error instanceof HttpErrorResponse && error.status === 404
              ? ""
              : getApiErrorMessage(
                  error,
                  "We could not load your roadmap right now.",
                );
          this.isLoadingExistingRoadmap = false;
        },
      });
  }

  private setRoadmapValues(response: RoadmapResponse): void {
    this.roadmapValues = [
      { label: "Roadmap ID", value: response.id },
      { label: "User ID", value: response.userId },
      ...this.fields.map((field) => ({
        label: field.label,
        value: response[field.id],
      })),
    ];
  }

  private toRequest(): RoadmapRequest {
    const origin = this.editAnswers.origin ?? "";
    const residencePermit =
      origin === "EU" ? "No" : (this.editAnswers.residencePermit ?? "No");
    const liveInSweden = this.editAnswers.liveInSweden ?? "No";
    const personnummer =
      liveInSweden === "Yes" ? (this.editAnswers.personnummer ?? "No") : "No";
    const appliedPersonnummer =
      personnummer === "No"
        ? (this.editAnswers.appliedPersonnummer ?? "No")
        : "Not applicable";
    const idCard =
      personnummer === "Yes" ? (this.editAnswers.idCard ?? "No") : "No";
    const bankAccount =
      idCard === "Yes" ? (this.editAnswers.bankAccount ?? "No") : "No";
    const bankId =
      bankAccount === "Yes" ? (this.editAnswers.bankId ?? "No") : "No";
    const planToDrive = this.editAnswers.planToDrive ?? "No";
    const drivingLicenceType =
      planToDrive === "Yes"
        ? (this.editAnswers.drivingLicenceType ?? "Other")
        : "Not applicable";

    return {
      origin,
      residencePermit,
      liveInSweden,
      personnummer,
      appliedPersonnummer,
      idCard,
      bankAccount,
      bankId,
      housing: this.editAnswers.housing ?? "",
      planToDrive,
      drivingLicenceType,
      purpose: this.editAnswers.purpose ?? "",
      insurance:
        liveInSweden === "Yes" ? (this.editAnswers.insurance ?? "No") : "No",
    };
  }

  toggleTask(task: Task): void {
    if (this.pendingTaskIds.has(task.id)) return;

    const previousState = task.completed;
    const nextState = !previousState;
    task.completed = nextState;
    this.pendingTaskIds.add(task.id);
    this.generationError = "";

    this.roadmapService
      .updateTaskCompletion(task.id, nextState)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.tasks = this.mapTasks(response.tasks ?? []);
          this.pendingTaskIds.delete(task.id);
        },
        error: (error: unknown) => {
          task.completed = previousState;
          this.pendingTaskIds.delete(task.id);
          this.generationError = getApiErrorMessage(
            error,
            "We could not update this task right now.",
          );
        },
      });
  }

  confirmDeleteRoadmap(): void {
    this.confirmationService.confirm({
      header: "Delete roadmap",
      message:
        "This will permanently delete your roadmap and checklist. Are you sure?",
      acceptLabel: "Delete",
      rejectLabel: "Cancel",
      acceptButtonProps: { severity: "danger" },
      rejectButtonProps: { severity: "secondary", outlined: true },
      accept: () => this.deleteRoadmap(),
    });
  }

  private deleteRoadmap(): void {
    if (this.isDeleting) return;

    this.isDeleting = true;
    this.generationError = "";
    this.successMessage = "";
    this.roadmapService
      .deleteRoadmap()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.hasExistingRoadmap = false;
          this.roadmapValues = [];
          this.tasks = [];
          this.isDeleting = false;
          this.successMessage = "Your roadmap has been deleted.";
        },
        error: (error: unknown) => {
          this.generationError = getApiErrorMessage(
            error,
            "We could not delete your roadmap right now.",
          );
          this.isDeleting = false;
        },
      });
  }

  private mapTasks(tasks: RoadmapTaskResponse[]): Task[] {
    return tasks.map((task) => ({
      id: task.id,
      title: task.title,
      description: task.description,
      completed: task.completed,
    }));
  }
}
