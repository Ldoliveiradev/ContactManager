import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { extractApiError } from '../../../../core/utils/api-error';
import { ToastService } from '../../../../core/services/toast.service';
import { AutofocusDirective } from '../../../../shared/directives/autofocus.directive';
import { PhoneMaskDirective } from '../../../../shared/directives/phone-mask.directive';
import { AlertComponent } from '../../../../shared/ui/alert/alert.component';
import { ButtonComponent } from '../../../../shared/ui/button/button.component';
import { CardComponent } from '../../../../shared/ui/card/card.component';
import { FormFieldComponent } from '../../../../shared/ui/form-field/form-field.component';
import * as v from '../../../../shared/validators/contact.validators';
import { CreateContactRequest } from '../../models/create-contact-request.interface';
import { UpdateContactRequest } from '../../models/update-contact-request.interface';
import { ContactService } from '../../services/contact.service';

@Component({
  selector: 'app-contact-form',
  imports: [ReactiveFormsModule, AutofocusDirective, PhoneMaskDirective, AlertComponent, ButtonComponent, CardComponent, FormFieldComponent],
  templateUrl: './contact-form.component.html',
  styleUrl: './contact-form.component.scss',
})
export class ContactForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly contacts = inject(ContactService);
  private readonly toast = inject(ToastService);

  readonly editId = input<string | null>(null);
  readonly saved = output<void>();
  readonly cancelled = output<void>();

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, v.notBlank(), v.trimmedLength(3, 100)]],
    email: ['', [Validators.required, v.email(), v.trimmedLength(6, 200)]],
    phone: ['', [v.phone()]],
  });

  private static readonly DISCARD_MESSAGE =
    'You have unsaved changes. Do you want to leave this form and discard them?';

  private initialFormValue = this.form.getRawValue();

  private static readonly MESSAGES: Record<string, Record<string, string>> = {
    name: {
      required: 'Name is required.',
      blank: 'Name is required.',
      minLengthTrimmed: 'Name must be at least 3 characters.',
      maxLengthTrimmed: 'Name must be 100 characters or fewer.',
    },
    email: {
      required: 'Email is required.',
      blank: 'Email is required.',
      email: 'Enter a valid email address.',
      minLengthTrimmed: 'Enter a valid email address.',
      maxLengthTrimmed: 'Email must be 200 characters or fewer.',
    },
    phone: {
      phone: 'Enter a valid phone number.',
    },
  };

  protected errorFor(name: 'name' | 'email' | 'phone'): string | null {
    const control: AbstractControl = this.form.controls[name];
    if (!control.touched || !control.errors) {
      return null;
    }
    const messages = ContactForm.MESSAGES[name];
    const firstKey = Object.keys(control.errors).find((key) => messages[key]);
    return firstKey ? messages[firstKey] : null;
  }

  ngOnInit(): void {
    const id = this.editId();
    if (id) {
      this.contacts.getById(id).subscribe({
        next: (response) => {
          const c = response.data;
          if (c) {
            // Store raw digits in the control; the mask directive handles display on input events.
            // Patch the phone input element's display value directly via the static formatter.
            const digits = (c.phone ?? '').replace(/\D/g, '');
            this.form.patchValue({ name: c.name, email: c.email, phone: digits });
            this.initialFormValue = this.form.getRawValue();
            this.form.markAsPristine();
            if (digits) {
              setTimeout(() => {
                const phoneEl = document.getElementById('phone') as HTMLInputElement | null;
                if (phoneEl) phoneEl.value = PhoneMaskDirective.format(digits);
              });
            }
          }
        },
        error: () => this.error.set('Failed to load contact.'),
      });
    }
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const raw = this.form.getRawValue();
    const phone = raw.phone.trim() === '' ? null : raw.phone;

    const id = this.editId();
    if (id) {
      const payload: UpdateContactRequest = { name: raw.name, email: raw.email, phone };
      this.contacts.update(id, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.show('Contact updated successfully.', 'success');
          this.saved.emit();
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(extractApiError(err, err?.status === 400 ? 'Please check the form fields.' : 'Failed to save contact.'));
        },
      });
    } else {
      const payload: CreateContactRequest = { name: raw.name, email: raw.email, phone };
      this.contacts.create(payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.show('Contact created successfully.', 'success');
          this.saved.emit();
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(extractApiError(err, err?.status === 400 ? 'Please check the form fields.' : 'Failed to save contact.'));
        },
      });
    }
  }

  protected cancel(): void {
    if (this.hasUnsavedChanges() && !window.confirm(ContactForm.DISCARD_MESSAGE)) {
      return;
    }
    this.cancelled.emit();
  }

  private hasUnsavedChanges(): boolean {
    const current = this.form.getRawValue();
    return current.name !== this.initialFormValue.name
      || current.email !== this.initialFormValue.email
      || current.phone !== this.initialFormValue.phone;
  }
}
