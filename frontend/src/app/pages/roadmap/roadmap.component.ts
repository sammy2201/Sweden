import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { QUESTIONS, FlowQuestion } from "./roadmap.questions";

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
  questions: FlowQuestion[] = QUESTIONS;

  answers: Record<string, string> = {};

  tasks: Task[] = [];

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
    this.tasks = [];

    this.tasks.push({
      title: "Register your address",
      description: "Register your address with Skatteverket.",
      status: "done",
    });

    // If user is not living in Sweden, skip personnummer/bankId flows
    const liveInSweden = this.answers["liveInSweden"] === "Yes";

    if (liveInSweden) {
      if (this.answers["personnummer"] === "No") {
        this.tasks.push({
          title: "Apply for Personnummer",
          description: "Book an appointment with Skatteverket.",
          status: "current",
        });
      }

      if (this.answers["personnummer"] === "Yes") {
        if (this.answers["idCard"] === "No") {
          this.tasks.push({
            title: "Apply for Swedish ID Card",
            description:
              "Order your ID card after receiving your personnummer.",
            status: "todo",
          });
        }

        if (
          this.answers["idCard"] === "Yes" &&
          this.answers["bankAccount"] === "No"
        ) {
          this.tasks.push({
            title: "Open a Swedish Bank Account",
            description: "Visit Nordea, SEB, Handelsbanken or Swedbank.",
            status: "todo",
          });
        }

        if (
          this.answers["bankAccount"] === "Yes" &&
          this.answers["bankid"] === "No"
        ) {
          this.tasks.push({
            title: "Activate BankID",
            description: "Set up BankID after opening your bank account.",
            status: "todo",
          });
        }

        if (this.answers["insurance"] === "No") {
          this.tasks.push({
            title: "Register with Försäkringskassan",
            description: "Apply for Swedish social insurance.",
            status: "todo",
          });
        }
      }
    }

    if (this.answers["purpose"] === "Looking for work") {
      this.tasks.push({
        title: "Register with Arbetsförmedlingen",
        description: "Create a profile and search for jobs.",
        status: "todo",
      });
    }

    if (this.answers["housing"] === "Still looking") {
      this.tasks.push({
        title: "Join Housing Queues",
        description: "Register with Boplats, HomeQ and your municipality.",
        status: "todo",
      });
    }

    if (
      this.answers["planToDrive"] === "Yes" &&
      this.answers["drivingLicenceType"] === "Other"
    ) {
      this.tasks.push({
        title: "Check Driving Licence Rules",
        description: "Find out whether your licence can be exchanged.",
        status: "todo",
      });
    }

    this.tasks.push({
      title: "Get a Public Transport Card",
      description: "Purchase a regional travel card.",
      status: "todo",
    });
  }
}
