import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ContactInput } from '../../core/models/contact.model';
import { ContactService } from '../../core/services/contact.service';
import { AutofocusDirective } from '../../shared/directives/autofocus.directive';
import { Alert } from '../../shared/ui/alert/alert';
import { Button } from '../../shared/ui/button/button';
import { Card } from '../../shared/ui/card/card';
import { FormField } from '../../shared/ui/form-field/form-field';
import * as v from '../../shared/validators/contact.validators';

@Component({
  selector: 'app-contact-form',
  imports: [ReactiveFormsModule, AutofocusDirective, Alert, Button, Card, FormField],
  templateUrl: './contact-form.html',
  styleUrl: './contact-form.scss',
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

  // Per-field error message tables keyed by validation error name.
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

  /** Maps a control's validation errors to a human-readable message. */
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
        next: (c) => this.form.patchValue({ name: c.name, email: c.email, phone: c.phone ?? '' }),
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
    const payload: ContactInput = {
      name: raw.name,
      email: raw.email,
      phone: raw.phone.trim() === '' ? null : raw.phone,
    };

    const id = this.editId();
    const request$ = id
      ? this.contacts.update(id, payload)
      : this.contacts.create(payload);

    request$.subscribe({
      next: () => this.router.navigate(['/contacts']),
      error: (err) => {
        this.saving.set(false);
        this.error.set(err?.status === 400 ? 'Please check the form fields.' : 'Failed to save contact.');
      },
    });
  }

  protected cancel(): void {
    this.router.navigate(['/contacts']);
  }
}
