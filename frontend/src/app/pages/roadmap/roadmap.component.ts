import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { QUESTIONS, FlowQuestion } from "./roadmap.questions";
import {
  RoadmapRequest,
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
export class RoadmapComponent {
  private readonly roadmapService = inject(RoadmapService);

  questions: FlowQuestion[] = QUESTIONS;

  answers: Record<string, string> = {};

  tasks: Task[] = [];
  isGenerating = false;
  generationError = "";

  currentIndex = 0;

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

    this.roadmapService.generateRoadmap(this.toRequest()).subscribe({
      next: (response) => {
        this.tasks = this.mapTasks(response.tasks ?? []);
        this.isGenerating = false;
      },
      error: () => {
        this.generationError =
          "We could not generate your roadmap right now. Please try again.";
        this.isGenerating = false;
      },
    });
  }

  private toRequest(): RoadmapRequest {
    return {
      origin: this.answers["origin"] ?? "",
      residencePermit: this.answers["residencePermit"] ?? "",
      liveInSweden: this.answers["liveInSweden"] ?? "",
      personnummer: this.answers["personnummer"] ?? "",
      appliedPersonnummer: this.answers["appliedPersonnummer"] ?? "",
      idCard: this.answers["idCard"] ?? "",
      bankAccount: this.answers["bankAccount"] ?? "",
      bankId: this.answers["bankId"] ?? "",
      housing: this.answers["housing"] ?? "",
      planToDrive: this.answers["planToDrive"] ?? "",
      drivingLicenceType: this.answers["drivingLicenceType"] ?? "",
      purpose: this.answers["purpose"] ?? "",
      insurance: this.answers["insurance"] ?? "",
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
