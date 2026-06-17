import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AlertComponent } from '../../../../shared/ui/alert/alert.component';
import { ButtonComponent } from '../../../../shared/ui/button/button.component';
import { CardComponent } from '../../../../shared/ui/card/card.component';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { PasswordStrengthComponent } from '../../../../shared/ui/password-strength/password-strength.component';
import { extractApiError } from '../../../../core/utils/api-error';
import { ToastService } from '../../../../core/services/toast.service';
import * as v from '../../../../shared/validators/contact.validators';
import { AccountDto } from '../../models/account-dto.interface';
import { AccountService } from '../../services/account.service';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, AlertComponent, ButtonComponent, CardComponent, FormFieldComponent, PasswordStrengthComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class Profile implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accounts = inject(AccountService);
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly account = input<AccountDto | null>(null);
  readonly profileUpdated = output<AccountDto | null>();

  protected readonly savingProfile = signal(false);
  protected readonly savingPassword = signal(false);
  protected readonly profileError = signal<string | null>(null);
  protected readonly passwordError = signal<string | null>(null);
  protected readonly newPasswordValue = signal('');

  protected readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, v.notBlank(), v.trimmedLength(2, 50)]],
    lastName: ['', [Validators.required, v.notBlank(), v.trimmedLength(2, 50)]],
    email: ['', [Validators.required, v.email(), v.trimmedLength(6, 200)]],
  });

  protected readonly passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(72), v.passwordStrength()]],
    confirmNewPassword: ['', [Validators.required]],
  }, { validators: v.passwordsMatch('newPassword', 'confirmNewPassword') });

  private static readonly PROFILE_MESSAGES: Record<string, Record<string, string>> = {
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
  };

  private static readonly PASSWORD_MESSAGES: Record<string, Record<string, string>> = {
    currentPassword: { required: 'Current password is required.' },
    newPassword: {
      required: 'New password is required.',
      minlength: 'Password must be at least 8 characters.',
      maxlength: 'Password must be 72 characters or fewer.',
      passwordStrength: 'Must contain at least one uppercase letter, one lowercase letter, and one digit.',
    },
    confirmNewPassword: {
      required: 'Please confirm your new password.',
      passwordsMismatch: 'Passwords do not match.',
    },
  };

  protected profileErrorFor(name: 'firstName' | 'lastName' | 'email'): string | null {
    const control = this.profileForm.controls[name];
    if (!control.touched || !control.errors) return null;
    const messages = Profile.PROFILE_MESSAGES[name];
    const key = Object.keys(control.errors).find((k) => messages[k]);
    return key ? messages[key] : null;
  }

  protected passwordErrorFor(name: 'currentPassword' | 'newPassword' | 'confirmNewPassword'): string | null {
    const control = this.passwordForm.controls[name];
    // For confirmNewPassword also check group-level error when control is touched
    const errors = name === 'confirmNewPassword'
      ? { ...control.errors, ...this.passwordForm.errors }
      : control.errors;
    if (!control.touched || !errors) return null;
    const messages = Profile.PASSWORD_MESSAGES[name];
    const key = Object.keys(errors).find((k) => messages[k]);
    return key ? messages[key] : null;
  }

  ngOnInit(): void {
    this.passwordForm.controls.newPassword.valueChanges.subscribe((v) => this.newPasswordValue.set(v));

    const data = this.account();
    if (data) {
      this.profileForm.patchValue({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
      });
    }
  }

  protected saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    this.savingProfile.set(true);
    this.profileError.set(null);

    this.accounts.updateProfile(this.profileForm.getRawValue()).subscribe({
      next: (response) => {
        this.savingProfile.set(false);
        this.toast.show('Profile updated.');
        this.profileUpdated.emit(response.data);
      },
      error: (err) => {
        this.savingProfile.set(false);
        this.profileError.set(extractApiError(err, 'Failed to update profile.'));
      },
    });
  }

  protected changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    const account = this.account();
    if (!account) {
      this.passwordError.set('Failed to determine the authenticated user.');
      return;
    }
    this.savingPassword.set(true);
    this.passwordError.set(null);

    this.authService.updatePassword({
      userId: account.id,
      ...this.passwordForm.getRawValue(),
    }).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.toast.show('Password changed.');
        this.passwordForm.reset();
      },
      error: (err) => {
        this.savingPassword.set(false);
        this.passwordError.set(extractApiError(err, 'Failed to change password.'));
      },
    });
  }
}
