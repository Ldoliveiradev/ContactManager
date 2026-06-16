import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AutofocusDirective } from '../../../../shared/directives/autofocus.directive';
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
  imports: [ReactiveFormsModule, AutofocusDirective, AlertComponent, ButtonComponent, CardComponent, FormFieldComponent],
  templateUrl: './contact-form.component.html',
  styleUrl: './contact-form.component.scss',
})
export class ContactForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly contacts = inject(ContactService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editId = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, v.notBlank(), v.trimmedLength(2, 200)]],
    email: ['', [Validators.required, v.email(), v.trimmedLength(3, 200)]],
    phone: ['', [v.phone()]],
  });

  private static readonly MESSAGES: Record<string, Record<string, string>> = {
    name: {
      required: 'Name is required.',
      blank: 'Name is required.',
      minLengthTrimmed: 'Name must be at least 2 characters.',
      maxLengthTrimmed: 'Name must be 200 characters or fewer.',
    },
    email: {
      required: 'Email is required.',
      blank: 'Email is required.',
      email: 'Enter a valid email address.',
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
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editId.set(id);
      this.contacts.getById(id).subscribe({
        next: (c) => {
          if (c) {
            this.form.patchValue({ name: c.name, email: c.email, phone: c.phone ?? '' });
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
        next: () => this.router.navigate(['/contacts']),
        error: (err) => {
          this.saving.set(false);
          this.error.set(err?.status === 400 ? 'Please check the form fields.' : 'Failed to save contact.');
        },
      });
    } else {
      const payload: CreateContactRequest = { name: raw.name, email: raw.email, phone };
      this.contacts.create(payload).subscribe({
        next: () => this.router.navigate(['/contacts']),
        error: (err) => {
          this.saving.set(false);
          this.error.set(err?.status === 400 ? 'Please check the form fields.' : 'Failed to save contact.');
        },
      });
    }
  }

  protected cancel(): void {
    this.router.navigate(['/contacts']);
  }
}
