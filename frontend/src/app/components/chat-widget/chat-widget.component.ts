import { Component } from "@angular/core";
import { ButtonModule } from "primeng/button";

interface ChatMessage {
  role: "assistant" | "user";
  text: string;
  time: string;
}

@Component({
  selector: "app-chat-widget",
  standalone: true,
  imports: [ButtonModule],
  templateUrl: "./chat-widget.component.html",
  styleUrl: "./chat-widget.component.css",
})
export class ChatWidgetComponent {
  isChatOpen = false;
  isSending = false;
  draftMessage = "";

  readonly chatMessages: ChatMessage[] = [
    {
      role: "assistant",
      text: "Hej! I am your Sweden Start assistant UI.",
      time: this.formatNow(),
    },
  ];

  openChat(): void {
    this.isChatOpen = true;
  }

  minimizeChat(): void {
    this.isChatOpen = false;
  }

  sendChatMessage(): void {
    const text = this.draftMessage.trim();
    if (!text || this.isSending) {
      return;
    }

    this.chatMessages.push({
      role: "user",
      text,
      time: this.formatNow(),
    });

    this.draftMessage = "";
    this.isSending = true;

    window.setTimeout(() => {
      this.chatMessages.push({
        role: "assistant",
        text: "Work In Progress....",
        time: this.formatNow(),
      });
      this.isSending = false;
    }, 500);
  }

  onChatInput(event: Event): void {
    const target = event.target as HTMLTextAreaElement;
    this.draftMessage = target.value;
  }

  onChatEnter(event: KeyboardEvent): void {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      this.sendChatMessage();
    }
  }

  private formatNow(): string {
    return new Date().toLocaleTimeString([], {
      hour: "2-digit",
      minute: "2-digit",
    });
  }
}
