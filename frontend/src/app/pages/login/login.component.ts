import { Component, inject } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { CommonModule } from "@angular/common";
import { AuthService } from "../../services/auth.service";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { PasswordModule } from "primeng/password";
import { getApiErrorMessage } from "../../utils/api-error";

@Component({
  selector: "app-login",
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
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.css",
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  showPassword = false;

  readonly form = this.fb.nonNullable.group({
    email: ["", [Validators.required, Validators.email]],
    password: ["", [Validators.required]],
  });

  isSubmitting = false;
  errorMessage = "";

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = "";

    try {
      await this.authService.login(this.form.getRawValue());
      const redirectTo = this.route.snapshot.queryParamMap.get("redirectTo");
      await this.router.navigateByUrl(redirectTo || "/");
    } catch (error: unknown) {
      this.errorMessage = getApiErrorMessage(
        error,
        "Invalid email or password.",
      );
    } finally {
      this.isSubmitting = false;
    }
  }
}
