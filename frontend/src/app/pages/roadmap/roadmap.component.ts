import { CommonModule } from "@angular/common";
import { Component, OnInit, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { QUESTIONS, FlowQuestion } from "./roadmap.questions";
import {
  RoadmapRequest,
  RoadmapResponse,
  RoadmapService,
  RoadmapTaskResponse,
} from "../../services/roadmap.service";

interface Task {
  title: string;
  description: string;
  status: "done" | "current" | "todo";
}

@Component({
  selector: "app-roadmap",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./roadmap.component.html",
  styleUrls: ["./roadmap.component.css"],
})
export class RoadmapComponent implements OnInit {
  private readonly roadmapService = inject(RoadmapService);

  questions: FlowQuestion[] = QUESTIONS;

  answers: Record<string, string> = {};

  tasks: Task[] = [];
  hasExistingRoadmap = false;
  isLoadingExistingRoadmap = true;
  isEditingExistingRoadmap = false;
  isGenerating = false;
  generationError = "";
  successMessage = "";
  private readonly roadmapExistsMessage =
    "Cannot create new roadmap. Go to profile and update your existing roadmap.";

  currentIndex = 0;

  ngOnInit(): void {
    this.loadExistingRoadmap();
  }

  get visibleQuestions(): FlowQuestion[] {
    return this.questions.filter((q) =>
      q.showIf ? q.showIf(this.answers) : true,
    );
  }

  get currentQuestion(): FlowQuestion | undefined {
    const questions = this.visibleQuestions;

    if (questions.length === 0) {
      return undefined;
    }

    if (this.currentIndex >= questions.length) {
      this.currentIndex = questions.length - 1;
    }

    return questions[this.currentIndex];
  }

  get isComplete(): boolean {
    return this.visibleQuestions.every((q) => Boolean(this.answers[q.id]));
  }

  get isLastQuestion(): boolean {
    return this.currentIndex >= this.visibleQuestions.length - 1;
  }

  get shouldShowWizard(): boolean {
    return this.isEditingExistingRoadmap || (!this.hasExistingRoadmap && !this.isComplete);
  }

  get primaryActionLabel(): string {
    if (!this.isLastQuestion) {
      return "Next";
    }

    return this.hasExistingRoadmap ? "Update Roadmap" : "Generate My Roadmap";
  }

  get progress(): number {
    const questions = this.visibleQuestions;

    if (!questions.length || questions.length === 1) {
      return 100;
    }

    return Math.round((this.currentIndex / (questions.length - 1)) * 100);
  }

  previous(): void {
    if (this.currentIndex > 0) {
      this.currentIndex -= 1;
    }
  }

  next(): void {
    const question = this.currentQuestion;

    if (!question || !this.answers[question.id]) {
      return;
    }

    const nextIndex = this.currentIndex + 1;

    if (nextIndex < this.visibleQuestions.length) {
      this.currentIndex = nextIndex;
      return;
    }

    this.generateRoadmap();
  }

  onAnswerChange(): void {
    const unansweredIndex = this.visibleQuestions.findIndex(
      (q) => !this.answers[q.id],
    );

    this.currentIndex =
      unansweredIndex >= 0 ? unansweredIndex : this.visibleQuestions.length;
  }

  generateRoadmap(): void {
    if (!this.isComplete || this.isGenerating) {
      return;
    }

    this.isGenerating = true;
    this.generationError = "";
    this.successMessage = "";

    const request = this.toRequest();
    const wasUpdating = this.hasExistingRoadmap;
    const saveRoadmap = wasUpdating
      ? this.roadmapService.updateRoadmap(request)
      : this.roadmapService.generateRoadmap(request);

    saveRoadmap.subscribe({
      next: (response) => {
        this.applyRoadmapAnswers(response);
        this.tasks = this.mapTasks(response.tasks ?? []);
        this.hasExistingRoadmap = true;
        this.isEditingExistingRoadmap = false;
        this.isGenerating = false;
        this.currentIndex = 0;
        this.successMessage = wasUpdating ? "Your roadmap has been updated." : "";
      },
      error: (error: HttpErrorResponse) => {
        this.generationError = this.getGenerationErrorMessage(error);

        this.isGenerating = false;
      },
    });
  }

  startRoadmapUpdate(): void {
    this.isEditingExistingRoadmap = true;
    this.currentIndex = 0;
    this.generationError = "";
    this.successMessage = "";
  }

  cancelRoadmapUpdate(): void {
    this.isEditingExistingRoadmap = false;
    this.currentIndex = 0;
    this.generationError = "";
  }

  private loadExistingRoadmap(): void {
    this.roadmapService.getRoadmap().subscribe({
      next: (response) => {
        if (response?.id) {
          this.applyRoadmapAnswers(response);
          this.tasks = this.mapTasks(response.tasks ?? []);
          this.hasExistingRoadmap = true;
        }

        this.isLoadingExistingRoadmap = false;
      },
      error: (error: HttpErrorResponse) => {
        if (error.status !== 404) {
          this.generationError = this.getGenerationErrorMessage(error);
        }

        this.isLoadingExistingRoadmap = false;
      },
    });
  }

  private getGenerationErrorMessage(error: HttpErrorResponse): string {
    const code = error?.error?.code;
    const backendMessage =
      typeof error?.error?.message === "string"
        ? error.error.message
        : undefined;

    if (error.status === 409 && code === "ROADMAP_ALREADY_EXISTS") {
      return this.roadmapExistsMessage;
    }

    if (error.status === 401) {
      return "Your session expired. Please log in again and retry.";
    }

    if (error.status === 403) {
      return "You are not allowed to create a roadmap for this account.";
    }

    if (error.status === 0) {
      return "Cannot reach the server. Make sure backend is running on http://localhost:5195.";
    }

    if (backendMessage) {
      return backendMessage;
    }

    return "We could not generate your roadmap right now. Please try again.";
  }

  private applyRoadmapAnswers(response: RoadmapResponse): void {
    this.answers = {
      origin: response.origin,
      residencePermit: response.residencePermit,
      liveInSweden: response.liveInSweden,
      personnummer: response.personnummer,
      appliedPersonnummer: response.appliedPersonnummer,
      idCard: response.idCard,
      bankAccount: response.bankAccount,
      bankId: response.bankId,
      housing: response.housing,
      planToDrive: response.planToDrive,
      drivingLicenceType: response.drivingLicenceType,
      purpose: response.purpose,
      insurance: response.insurance,
    };
  }

  private toRequest(): RoadmapRequest {
    const origin = this.answers["origin"] ?? "";
    const residencePermit =
      origin === "EU" ? "No" : (this.answers["residencePermit"] ?? "No");

    const liveInSweden = this.answers["liveInSweden"] ?? "No";
    const personnummer =
      liveInSweden === "Yes" ? (this.answers["personnummer"] ?? "No") : "No";

    const appliedPersonnummer =
      personnummer === "No"
        ? (this.answers["appliedPersonnummer"] ?? "No")
        : "Not applicable";

    const idCard =
      personnummer === "Yes" ? (this.answers["idCard"] ?? "No") : "No";

    const bankAccount =
      idCard === "Yes" ? (this.answers["bankAccount"] ?? "No") : "No";

    const bankId =
      bankAccount === "Yes" ? (this.answers["bankId"] ?? "No") : "No";

    const planToDrive = this.answers["planToDrive"] ?? "No";
    const drivingLicenceType =
      planToDrive === "Yes"
        ? (this.answers["drivingLicenceType"] ?? "Other")
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
      housing: this.answers["housing"] ?? "",
      planToDrive,
      drivingLicenceType,
      purpose: this.answers["purpose"] ?? "",
      insurance:
        liveInSweden === "Yes" ? (this.answers["insurance"] ?? "No") : "No",
    };
  }

  private mapTasks(tasks: RoadmapTaskResponse[]): Task[] {
    let currentMarked = false;

    return tasks.map((task) => {
      if (task.completed) {
        return {
          title: task.title,
          description: task.description,
          status: "done" as const,
        };
      }

      if (!currentMarked) {
        currentMarked = true;
        return {
          title: task.title,
          description: task.description,
          status: "current" as const,
        };
      }

      return {
        title: task.title,
        description: task.description,
        status: "todo" as const,
      };
    });
  }
}
