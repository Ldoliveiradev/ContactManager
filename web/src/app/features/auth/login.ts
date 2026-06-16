import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly mode = signal<'login' | 'register'>('login');

  protected readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected toggleMode(): void {
    this.mode.set(this.mode() === 'login' ? 'register' : 'login');
    this.error.set(null);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    const credentials = this.form.getRawValue();

    if (this.mode() === 'login') {
      this.auth.login(credentials).subscribe({
        next: () => this.router.navigate(['/contacts']),
        error: (err: { status?: number }) => this.handleError(err),
      });
      return;
    }

    // Register, then log in to obtain a token.
    this.auth.register(credentials).subscribe({
      next: () =>
        this.auth.login(credentials).subscribe({
          next: () => this.router.navigate(['/contacts']),
          error: (err: { status?: number }) => this.handleError(err),
        }),
      error: (err: { status?: number }) => this.handleError(err),
    });
  }

  private handleError(err?: { status?: number }): void {
    this.loading.set(false);
    this.error.set(
      this.mode() === 'login'
        ? 'Invalid username or password.'
        : err?.status === 409
          ? 'That username is already taken.'
          : 'Registration failed. Please try again.',
    );
  }
}
