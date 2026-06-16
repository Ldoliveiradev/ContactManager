import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faEnvelope,
  faPenToSquare,
  faPhone,
  faPlus,
  faTrashCan,
} from '@fortawesome/free-solid-svg-icons';
import { Contact } from '../../core/models/contact.model';
import { ContactService } from '../../core/services/contact.service';
import { PhonePipe } from '../../shared/pipes/phone.pipe';
import { Alert } from '../../shared/ui/alert/alert';
import { Button } from '../../shared/ui/button/button';
import { Grid } from '../../shared/ui/grid/grid';
import { GridActionsDirective } from '../../shared/ui/grid/grid-actions.directive';
import { GridCellDirective } from '../../shared/ui/grid/grid-cell.directive';
import { GridColumn } from '../../shared/ui/grid/grid.types';

@Component({
  selector: 'app-contact-list',
  imports: [
    RouterLink,
    PhonePipe,
    FaIconComponent,
    Alert,
    Button,
    Grid,
    GridCellDirective,
    GridActionsDirective,
  ],
  templateUrl: './contact-list.html',
  styleUrl: './contact-list.scss',
})
export class ContactList implements OnInit {
  private readonly contacts = inject(ContactService);
  private readonly router = inject(Router);

  // FontAwesome icons used in the template.
  protected readonly faPlus = faPlus;
  protected readonly faEnvelope = faEnvelope;
  protected readonly faPhone = faPhone;
  protected readonly faPenToSquare = faPenToSquare;
  protected readonly faTrashCan = faTrashCan;

  protected readonly items = signal<Contact[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  // Column definitions for the generic grid.
  protected readonly columns: GridColumn<Contact>[] = [
    { key: 'name', header: 'Name', sortable: true, searchable: true, cell: 'name' },
    { key: 'email', header: 'Email', sortable: true, searchable: true, cell: 'email' },
    { key: 'phone', header: 'Phone', searchable: true, cell: 'phone' },
  ];

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.contacts.getAll().subscribe({
      next: (data) => {
        this.items.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load contacts.');
        this.loading.set(false);
      },
    });
  }

  protected editContact(contact: Contact): void {
    this.router.navigate(['/contacts', contact.id, 'edit']);
  }

  protected remove(contact: Contact): void {
    if (!confirm(`Delete ${contact.name}?`)) {
      return;
    }
    this.contacts.delete(contact.id).subscribe({
      next: () => this.items.update((list) => list.filter((c) => c.id !== contact.id)),
      error: () => this.error.set('Failed to delete contact.'),
    });
  }
}
