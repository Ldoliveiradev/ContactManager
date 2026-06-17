import { Component, inject, output, signal, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AutofocusDirective } from '../../../../shared/directives/autofocus.directive';
import { AlertComponent } from '../../../../shared/ui/alert/alert.component';
import { ButtonComponent } from '../../../../shared/ui/button/button.component';
import { CardComponent } from '../../../../shared/ui/card/card.component';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { PasswordStrengthComponent } from '../../../../shared/ui/password-strength/password-strength.component';
import { extractApiError } from '../../../../core/utils/api-error';
import * as v from '../../../../shared/validators/contact.validators';
import { RegisterRequest } from '../../models/register-request.interface';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, AutofocusDirective, AlertComponent, ButtonComponent, CardComponent, FormFieldComponent, PasswordStrengthComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class Register implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly switchToLogin = output<void>();

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly passwordValue = signal('');

  ngOnInit(): void {
    this.form.controls.password.valueChanges.subscribe((v) => this.passwordValue.set(v));
  }

  protected readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required, v.notBlank(), v.trimmedLength(3, 100)]],
    firstName: ['', [Validators.required, v.notBlank(), v.trimmedLength(2, 50)]],
    lastName: ['', [Validators.required, v.notBlank(), v.trimmedLength(2, 50)]],
    email: ['', [Validators.required, v.email(), v.trimmedLength(6, 200)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(72), v.passwordStrength()]],
    confirmPassword: ['', [Validators.required]],
  }, { validators: v.passwordsMatch('password', 'confirmPassword') });

  private static readonly MESSAGES: Record<string, Record<string, string>> = {
    username: {
      required: 'Username is required.',
      blank: 'Username is required.',
      minLengthTrimmed: 'Username must be at least 3 characters.',
      maxLengthTrimmed: 'Username must be 100 characters or fewer.',
    },
    firstName: {
      required: 'First name is required.',
      blank: 'First name is required.',
      minLengthTrimmed: 'First name must be at least 2 characters.',
      maxLengthTrimmed: 'First name must be 50 characters or fewer.',
    },
    lastName: {
      required: 'Last name is required.',
      blank: 'Last name is required.',
      minLengthTrimmed: 'Last name must be at least 2 characters.',
      maxLengthTrimmed: 'Last name must be 50 characters or fewer.',
    },
    email: {
      required: 'Email is required.',
      blank: 'Email is required.',
      email: 'Enter a valid email address.',
      minLengthTrimmed: 'Enter a valid email address.',
      maxLengthTrimmed: 'Email must be 200 characters or fewer.',
    },
    password: {
      required: 'Password is required.',
      minlength: 'Password must be at least 8 characters.',
      maxlength: 'Password must be 72 characters or fewer.',
      passwordStrength: 'Must contain at least one uppercase letter, one lowercase letter, and one digit.',
    },
    confirmPassword: {
      required: 'Please confirm your password.',
      passwordsMismatch: 'Passwords do not match.',
    },
  };

  protected errorFor(name: 'username' | 'firstName' | 'lastName' | 'email' | 'password' | 'confirmPassword'): string | null {
    const control: AbstractControl = this.form.controls[name];
    const errors = name === 'confirmPassword'
      ? { ...control.errors, ...this.form.errors }
      : control.errors;
    if (!control.touched || !errors) return null;
    const messages = Register.MESSAGES[name];
    const firstKey = Object.keys(errors).find((key) => messages[key]);
    return firstKey ? messages[firstKey] : null;
  }

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
      error: (err: { status?: number; error?: { detail?: string; title?: string } }) => {
        this.loading.set(false);
        this.error.set(extractApiError(err, 'Registration failed. Please try again.'));
      },
    });
  }
}
