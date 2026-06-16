import { Component, inject, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AutofocusDirective } from '../../../../shared/directives/autofocus.directive';
import { AlertComponent } from '../../../../shared/ui/alert/alert.component';
import { ButtonComponent } from '../../../../shared/ui/button/button.component';
import { CardComponent } from '../../../../shared/ui/card/card.component';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { RegisterRequest } from '../../models/register-request.interface';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, AutofocusDirective, AlertComponent, ButtonComponent, CardComponent, FormFieldComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly switchToLogin = output<void>();

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    const raw = this.form.getRawValue();
    const payload: RegisterRequest = {
      username: raw.username,
      firstName: raw.firstName,
      lastName: raw.lastName,
      email: raw.email,
      password: raw.password,
    };
    this.auth.register(payload).subscribe({
      next: () =>
        this.auth.login({ username: raw.username, password: raw.password }).subscribe({
          next: () => this.router.navigate(['/contacts']),
          error: () => {
            this.loading.set(false);
            this.error.set('Registration succeeded but login failed. Please sign in.');
          },
        }),
      error: (err: { status?: number }) => {
        this.loading.set(false);
        this.error.set(err?.status === 409 ? 'That username is already taken.' : 'Registration failed. Please try again.');
      },
    });
  }
}
