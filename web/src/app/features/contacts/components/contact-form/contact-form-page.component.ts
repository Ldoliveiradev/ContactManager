import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ContactForm } from './contact-form.component';

@Component({
  selector: 'app-contact-form-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ContactForm],
  template: `
    <app-contact-form
      [editId]="editId"
      (saved)="onDone()"
      (cancelled)="onDone()"
    />
  `,
})
export class ContactFormPageComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly editId = this.route.snapshot.paramMap.get('id');

  protected onDone(): void {
    this.router.navigate(['/contacts']);
  }
}
