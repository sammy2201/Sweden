import { CommonModule } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { Component, DestroyRef, OnInit, inject } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormsModule, NgModel } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ConfirmationService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CheckboxModule } from "primeng/checkbox";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { DialogModule } from "primeng/dialog";
import { ProgressBarModule } from "primeng/progressbar";
import { SelectModule } from "primeng/select";
import {
  RoadmapQuestion,
  RoadmapQuestionDependency,
  RoadmapRequest,
  RoadmapResponse,
  RoadmapService,
  RoadmapTaskResponse,
} from "../../services/roadmap.service";
import { getApiErrorMessage } from "../../utils/api-error";
import { GuideLink } from "../../services/guide-directory.service";
import { RoadmapGuideService } from "../../services/roadmap-guide.service";

type RoadmapField = RoadmapQuestion;

interface RoadmapPhase {
  id: string;
  title: string;
  description: string;
  tasks: RoadmapTaskResponse[];
  completedCount: number;
  taskCount: number;
}

@Component({
  selector: "app-roadmap",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ButtonModule,
    CheckboxModule,
    ConfirmDialogModule,
    DialogModule,
    ProgressBarModule,
    SelectModule,
  ],
  providers: [ConfirmationService],
  templateUrl: "./roadmap.component.html",
  styleUrls: ["./roadmap.component.css"],
})
export class RoadmapComponent implements OnInit {
  private readonly roadmapService = inject(RoadmapService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly roadmapGuideService = inject(RoadmapGuideService);

  fields: RoadmapField[] = [];
  editAnswers: Partial<Record<keyof RoadmapRequest, string>> = {};
  tasks: RoadmapTaskResponse[] = [];
  hasExistingRoadmap = false;
  isLoadingExistingRoadmap = true;
  isLoadingQuestions = true;
  isEditingExistingRoadmap = false;
  isCreatingRoadmap = false;
  isGenerating = false;
  isDeleting = false;
  pendingTaskIds = new Set<string>();
  selectedTask: RoadmapTaskResponse | null = null;
  selectedCompletion = true;
  taskDialogError = "";
  generationError = "";
  successMessage = "";
  private currentRoadmapResponse: RoadmapResponse | null = null;

  get visibleFields(): RoadmapField[] {
    return this.fields.filter((field) => this.isVisible(field));
  }

  get applicableTasks(): RoadmapTaskResponse[] {
    return this.tasks.filter((task) => task.state !== "NotApplicable");
  }

  get completedTaskCount(): number {
    return this.applicableTasks.filter(
      (task) => task.completed || task.state === "Completed",
    ).length;
  }

  get remainingTaskCount(): number {
    return this.applicableTasks.filter(
      (task) =>
        !task.completed &&
        task.state !== "Completed" &&
        task.state !== "Waiting" &&
        this.unmetPrerequisiteTitles(task).length === 0,
    ).length;
  }

  get waitingTaskCount(): number {
    return this.applicableTasks.filter(
      (task) =>
        !task.completed &&
        (task.state === "Waiting" ||
          this.unmetPrerequisiteTitles(task).length > 0),
    ).length;
  }

  get taskProgressPercent(): number {
    if (this.applicableTasks.length === 0) return 0;
    return Math.round(
      (this.completedTaskCount / this.applicableTasks.length) * 100,
    );
  }

  get nextTask(): RoadmapTaskResponse | null {
    return (
      this.tasks.find(
        (task) =>
          task.state !== "NotApplicable" &&
          !task.completed &&
          task.state !== "Completed" &&
          task.state !== "Waiting" &&
          this.unmetPrerequisiteTitles(task).length === 0,
      ) ??
      this.tasks.find((task) => task.state === "Waiting" && !task.completed) ??
      null
    );
  }

  get phases(): RoadmapPhase[] {
    const phases = new Map<string, RoadmapPhase>();
    for (const task of this.tasks) {
      const phaseInfo = this.phaseFor(task.category);
      let phase = phases.get(phaseInfo.id);
      if (!phase) {
        phase = { ...phaseInfo, tasks: [], completedCount: 0, taskCount: 0 };
        phases.set(phaseInfo.id, phase);
      }
      phase.tasks.push(task);
      if (task.state !== "NotApplicable") {
        phase.taskCount += 1;
        if (task.completed || task.state === "Completed")
          phase.completedCount += 1;
      }
    }
    return [...phases.values()];
  }

  get isRoadmapDialogVisible(): boolean {
    return this.isEditingExistingRoadmap || this.isCreatingRoadmap;
  }

  set isRoadmapDialogVisible(visible: boolean) {
    if (!visible) this.cancelRoadmapUpdate();
  }

  ngOnInit(): void {
    this.roadmapService
      .getQuestions()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (questions) => {
          this.fields = [...questions].sort((a, b) => a.order - b.order);
          this.isLoadingQuestions = false;
        },
        error: (error: unknown) => {
          this.generationError = getApiErrorMessage(
            error,
            "We could not load the roadmap questions.",
          );
          this.isLoadingQuestions = false;
        },
      });
    this.loadExistingRoadmap();
  }

  startRoadmapUpdate(): void {
    if (!this.currentRoadmapResponse || this.fields.length === 0) return;
    this.editAnswers = {};
    for (const field of this.fields) {
      this.editAnswers[field.id] = this.currentRoadmapResponse[field.id] ?? "";
    }
    this.generationError = "";
    this.successMessage = "";
    this.isEditingExistingRoadmap = true;
    this.isCreatingRoadmap = false;
  }

  startRoadmapCreation(): void {
    this.editAnswers = {};
    for (const field of this.fields) this.editAnswers[field.id] = "";
    this.generationError = "";
    this.successMessage = "";
    this.isCreatingRoadmap = true;
    this.isEditingExistingRoadmap = false;
  }

  cancelRoadmapUpdate(): void {
    if (this.isGenerating) return;
    this.isEditingExistingRoadmap = false;
    this.isCreatingRoadmap = false;
    this.editAnswers = {};
    this.generationError = "";
  }

  saveRoadmap(): void {
    if (this.isGenerating || this.fields.length === 0) return;

    this.isGenerating = true;
    this.generationError = "";
    this.successMessage = "";
    const request = this.toRequest();
    const saveRequest = this.hasExistingRoadmap
      ? this.roadmapService.updateRoadmap(request)
      : this.roadmapService.generateRoadmap(request);

    saveRequest.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (response) => {
        this.applyRoadmapResponse(response);
        this.isEditingExistingRoadmap = false;
        this.isCreatingRoadmap = false;
        this.isGenerating = false;
        this.successMessage = "Your roadmap has been updated.";
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
            this.applyRoadmapResponse(response);
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

  private applyRoadmapResponse(response: RoadmapResponse): void {
    this.currentRoadmapResponse = response;
    this.tasks = response.tasks ?? [];
    this.hasExistingRoadmap = true;
  }

  private toRequest(): RoadmapRequest {
    const liveInSweden = this.editAnswers.liveInSweden ?? "";
    const personnummer =
      liveInSweden === "Yes"
        ? (this.editAnswers.personnummer ?? "")
        : "Not applicable";
    const planToDrive = this.editAnswers.planToDrive ?? "";
    return {
      origin: this.editAnswers.origin ?? "",
      residencePermit: this.editAnswers.residencePermit ?? "",
      liveInSweden,
      personnummer,
      appliedPersonnummer:
        personnummer === "No"
          ? (this.editAnswers.appliedPersonnummer ?? "")
          : "Not applicable",
      idCard:
        liveInSweden === "Yes"
          ? (this.editAnswers.idCard ?? "")
          : "Not applicable",
      bankAccount:
        liveInSweden === "Yes"
          ? (this.editAnswers.bankAccount ?? "")
          : "Not applicable",
      bankId:
        liveInSweden === "Yes"
          ? (this.editAnswers.bankId ?? "")
          : "Not applicable",
      housing: this.editAnswers.housing ?? "",
      planToDrive,
      drivingLicenceType:
        planToDrive === "Yes"
          ? (this.editAnswers.drivingLicenceType ?? "")
          : "Not applicable",
      purpose: this.editAnswers.purpose ?? "",
      insurance:
        liveInSweden === "Yes"
          ? (this.editAnswers.insurance ?? "")
          : "Not applicable",
    };
  }

  private isVisible(field: RoadmapField): boolean {
    const showRules = field.dependencies.filter(
      (dependency) => dependency.effect === "Show",
    );
    const hideRules = field.dependencies.filter(
      (dependency) => dependency.effect === "Hide",
    );
    const matches = (dependency: RoadmapQuestionDependency) =>
      dependency.operator === "Equals" &&
      this.editAnswers[dependency.sourceQuestionId] === dependency.answer;
    return (
      (showRules.length === 0 || showRules.some(matches)) &&
      !hideRules.some(matches)
    );
  }

  clarificationMessages(field: RoadmapField): string[] {
    return field.dependencies
      .filter((dependency) => dependency.effect === "RequireClarification")
      .filter(
        (dependency) =>
          dependency.operator === "Equals" &&
          this.editAnswers[dependency.sourceQuestionId] === dependency.answer,
      )
      .map((dependency) => dependency.explanation);
  }

  toggleTaskCompletion(
    task: RoadmapTaskResponse,
    completed: boolean,
    checkbox: NgModel,
  ): void {
    const savedCompletion = task.completed || task.state === "Completed";
    checkbox.control.setValue(savedCompletion, { emitEvent: false });
    if (
      this.pendingTaskIds.has(task.id) ||
      task.state === "NotApplicable" ||
      savedCompletion === completed
    )
      return;
    this.openTaskCompletionDialog(task, completed);
  }

  openTaskCompletionDialog(
    task: RoadmapTaskResponse,
    completed: boolean,
  ): void {
    if (!this.canChangeTask(task, completed)) return;
    this.selectedTask = task;
    this.selectedCompletion = completed;
    this.taskDialogError = "";
  }

  closeTaskCompletionDialog(): void {
    if (this.selectedTask && this.pendingTaskIds.has(this.selectedTask.id))
      return;
    this.selectedTask = null;
    this.taskDialogError = "";
  }

  confirmTaskCompletion(): void {
    const task = this.selectedTask;
    if (!task || !this.canChangeTask(task, this.selectedCompletion)) return;

    this.pendingTaskIds.add(task.id);
    this.taskDialogError = "";
    this.roadmapService
      .updateTaskCompletion(task.id, this.selectedCompletion)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.applyRoadmapResponse(response);
          this.selectedTask = null;
          this.successMessage = this.selectedCompletion
            ? "Step marked complete. Your roadmap progress is up to date."
            : "Step marked as not done. Your roadmap progress is up to date.";
        },
        error: (error: unknown) => {
          this.taskDialogError = getApiErrorMessage(
            error,
            "We could not update this step. Your saved progress has not changed.",
          );
        },
      })
      .add(() => this.pendingTaskIds.delete(task.id));
  }

  guideForTask(task: RoadmapTaskResponse): GuideLink | null {
    return this.roadmapGuideService.forTaskRule(task.ruleId);
  }

  unmetPrerequisiteTitles(task: RoadmapTaskResponse): string[] {
    const taskByRule = new Map(
      this.tasks.map((candidate) => [candidate.ruleId, candidate]),
    );
    return task.prerequisites.flatMap((prerequisite) => {
      const required = taskByRule.get(prerequisite.ruleId);
      return required &&
        required.state !== "NotApplicable" &&
        !required.completed &&
        required.state !== "Completed"
        ? [required.title]
        : [];
    });
  }

  completedDependentTitles(task: RoadmapTaskResponse): string[] {
    return this.tasks
      .filter(
        (candidate) =>
          (candidate.completed || candidate.state === "Completed") &&
          candidate.state !== "NotApplicable" &&
          candidate.prerequisites.some(
            (prerequisite) => prerequisite.ruleId === task.ruleId,
          ),
      )
      .map((candidate) => candidate.title);
  }

  canChangeTask(task: RoadmapTaskResponse, completed: boolean): boolean {
    if (
      task.state === "NotApplicable" ||
      task.state === "Waiting" ||
      this.pendingTaskIds.has(task.id)
    )
      return false;
    if (completed) return this.unmetPrerequisiteTitles(task).length === 0;
    return this.completedDependentTitles(task).length === 0;
  }

  taskActionBlockReason(task: RoadmapTaskResponse): string {
    if (task.state === "NotApplicable")
      return "This step does not apply to your answers.";
    if (task.state === "Waiting")
      return "This step is waiting for an update before it can be completed.";
    const prerequisites = this.unmetPrerequisiteTitles(task);
    if (prerequisites.length)
      return `Complete ${prerequisites.join(", ")} first.`;
    return `First mark ${this.completedDependentTitles(task).join(", ")} as not done.`;
  }

  isCurrentTask(task: RoadmapTaskResponse): boolean {
    return this.nextTask?.id === task.id;
  }

  statusLabel(task: RoadmapTaskResponse): string {
    if (task.state === "NotApplicable") return "Not applicable";
    if (task.completed || task.state === "Completed") return "Completed";
    if (this.unmetPrerequisiteTitles(task).length > 0) return "Blocked";
    if (task.state === "Applicable") return "Next step";
    return task.state;
  }

  isTaskBlocked(task: RoadmapTaskResponse): boolean {
    return (
      task.state !== "NotApplicable" &&
      !task.completed &&
      task.state !== "Completed" &&
      this.unmetPrerequisiteTitles(task).length > 0
    );
  }

  priorityLabel(priority: number): string {
    if (priority <= 1) return "High priority";
    if (priority === 2) return "Medium priority";
    if (priority === 3) return "Recommended";
    return "Optional";
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
          this.currentRoadmapResponse = null;
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

  private phaseFor(
    category: string,
  ): Pick<RoadmapPhase, "id" | "title" | "description"> {
    const normalized = category.trim().toLowerCase();
    if (["residence", "housing", "settlement"].includes(normalized)) {
      return {
        id: "move-planning",
        title: "Plan your move",
        description:
          "Prepare the practical pieces that shape your move and arrival.",
      };
    }
    if (
      ["registration", "identity", "banking", "insurance"].includes(normalized)
    ) {
      return {
        id: "essentials",
        title: "Get established",
        description:
          "Work through the services and registrations that apply to you.",
      };
    }
    if (["work", "study", "family", "transport"].includes(normalized)) {
      return {
        id: "daily-life",
        title: "Settle into daily life",
        description:
          "Continue with the work, study, family, or travel steps in your plan.",
      };
    }
    const title = category.trim() || "Other steps";
    return {
      id: `category-${title.toLowerCase().replace(/[^a-z0-9]+/g, "-")}`,
      title,
      description: "Additional steps from your personalized roadmap.",
    };
  }
}
