import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AlertComponent } from '../../../../shared/ui/alert/alert.component';
import { ButtonComponent } from '../../../../shared/ui/button/button.component';
import { CardComponent } from '../../../../shared/ui/card/card.component';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import { AccountDto } from '../../models/account-dto.interface';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, AlertComponent, ButtonComponent, CardComponent, FormFieldComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class Profile implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accounts = inject(AccountService);

  protected readonly loading = signal(true);
  protected readonly savingProfile = signal(false);
  protected readonly savingPassword = signal(false);
  protected readonly profileError = signal<string | null>(null);
  protected readonly profileSuccess = signal<string | null>(null);
  protected readonly passwordError = signal<string | null>(null);
  protected readonly passwordSuccess = signal<string | null>(null);
  protected readonly account = signal<AccountDto | null>(null);

  protected readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
  });

  protected readonly passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  ngOnInit(): void {
    this.accounts.getProfile().subscribe({
      next: (data) => {
        this.account.set(data);
        if (data) {
          this.profileForm.patchValue({
            firstName: data.firstName,
            lastName: data.lastName,
            email: data.email,
          });
        }
        this.loading.set(false);
      },
      error: () => {
        this.profileError.set('Failed to load profile.');
        this.loading.set(false);
      },
    });
  }

  protected saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    this.savingProfile.set(true);
    this.profileError.set(null);
    this.profileSuccess.set(null);

    this.accounts.updateProfile(this.profileForm.getRawValue()).subscribe({
      next: (data) => {
        this.account.set(data);
        this.savingProfile.set(false);
        this.profileSuccess.set('Profile updated.');
      },
      error: (err) => {
        this.savingProfile.set(false);
        this.profileError.set(
          err?.status === 400 ? 'Please check the form fields.' : 'Failed to update profile.',
        );
      },
    });
  }

  protected changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    this.savingPassword.set(true);
    this.passwordError.set(null);
    this.passwordSuccess.set(null);

    this.accounts.updatePassword(this.passwordForm.getRawValue()).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.passwordSuccess.set('Password changed.');
        this.passwordForm.reset();
      },
      error: (err) => {
        this.savingPassword.set(false);
        this.passwordError.set(
          err?.status === 400 ? 'Current password is incorrect.' : 'Failed to change password.',
        );
      },
    });
  }
}
