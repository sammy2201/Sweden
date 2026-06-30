import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { FormsModule } from "@angular/forms";

interface Question {
  key: string;
  title: string;
  options: string[];
}

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
  questions: Question[] = [
    {
      key: "origin",
      title: "Where are you from?",
      options: ["EU", "Non-EU"],
    },
    {
      key: "purpose",
      title: "Why are you in Sweden?",
      options: ["Work", "Study", "Family"],
    },
    {
      key: "city",
      title: "Where do you live?",
      options: ["Malmö", "Stockholm", "Gothenburg", "Other"],
    },
    {
      key: "personnummer",
      title: "Do you already have a Personnummer?",
      options: ["Yes", "No"],
    },
    {
      key: "bankid",
      title: "Do you have BankID?",
      options: ["Yes", "No"],
    },
  ];

  answers: Record<string, string> = {};

  tasks: Task[] = [];

  generateRoadmap() {
    this.tasks = [
      {
        title: "Register your address",
        description: "Register where you live.",
        status: "done",
      },
      {
        title: "Apply for Personnummer",
        description: "Book an appointment with Skatteverket.",
        status: this.answers["personnummer"] === "Yes" ? "done" : "current",
      },
      {
        title: "Open a Bank Account",
        description: "Visit a Swedish bank.",
        status: "todo",
      },
      {
        title: "Get BankID",
        description: "Activate BankID after opening your account.",
        status: this.answers["bankid"] === "Yes" ? "done" : "todo",
      },
      {
        title: "Register with Försäkringskassan",
        description: "Access Swedish social insurance.",
        status: "todo",
      },
      {
        title: "Join Housing Queue",
        description: "Start collecting queue days.",
        status: "todo",
      },
      {
        title: "Get Public Transport Card",
        description: "Purchase your regional travel card.",
        status: "todo",
      },
    ];
  }
}
