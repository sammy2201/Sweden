import { Component, inject } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { CommonModule } from "@angular/common";
import { AuthService } from "../../services/auth.service";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { PasswordModule } from "primeng/password";
import { getApiErrorMessage } from "../../utils/api-error";

@Component({
  selector: "app-register",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    CardModule,
    ButtonModule,
    PasswordModule,
    DividerModule,
  ],
  templateUrl: "./register.component.html",
  styleUrl: "./register.component.css",
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  showPassword = false;

  readonly form = this.fb.nonNullable.group({
    firstName: ["", [Validators.required, Validators.minLength(2)]],
    lastName: ["", [Validators.required, Validators.minLength(2)]],
    username: ["", [Validators.required, Validators.minLength(3)]],
    email: ["", [Validators.required, Validators.email]],
    city: ["", [Validators.required, Validators.minLength(2)]],
    address: ["", [Validators.required, Validators.minLength(3)]],
    password: ["", [Validators.required, Validators.minLength(8)]],
  });

  isSubmitting = false;
  errorMessage = "";
  successMessage = "";

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = "";
    this.successMessage = "";

    try {
      await this.authService.register(this.form.getRawValue());
      this.successMessage = "Account created successfully. You can now log in.";
      this.form.reset();
      setTimeout(() => this.router.navigate(["/login"]), 800);
    } catch (error: unknown) {
      this.errorMessage = getApiErrorMessage(
        error,
        "We could not create your account. Please try again.",
      );
    } finally {
      this.isSubmitting = false;
    }
  }
}
