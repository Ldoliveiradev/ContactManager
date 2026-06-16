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
import { PhonePipe } from '../../../../shared/pipes/phone.pipe';
import { Alert } from '../../../../shared/ui/alert/alert';
import { Button } from '../../../../shared/ui/button/button';
import { DataView } from '../../../../shared/ui/data-view/data-view';
import { DataViewActionsDirective } from '../../../../shared/ui/data-view/data-view-actions.directive';
import { DataViewCellDirective } from '../../../../shared/ui/data-view/data-view-cell.directive';
import { DataViewColumn } from '../../../../shared/ui/data-view/data-view.types';
import { ContactDto } from '../../models/contact-dto.interface';
import { ContactService } from '../../services/contact.service';

@Component({
  selector: 'app-contact-list',
  imports: [
    RouterLink,
    PhonePipe,
    FaIconComponent,
    Alert,
    Button,
    DataView,
    DataViewCellDirective,
    DataViewActionsDirective,
  ],
  templateUrl: './contact-list.html',
  styleUrl: './contact-list.scss',
})
export class ContactList implements OnInit {
  private readonly contacts = inject(ContactService);
  private readonly router = inject(Router);

  protected readonly faPlus = faPlus;
  protected readonly faEnvelope = faEnvelope;
  protected readonly faPhone = faPhone;
  protected readonly faPenToSquare = faPenToSquare;
  protected readonly faTrashCan = faTrashCan;

  protected readonly items = signal<ContactDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly columns: DataViewColumn<ContactDto>[] = [
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

  protected editContact(contact: ContactDto): void {
    this.router.navigate(['/contacts', contact.id, 'edit']);
  }

  protected remove(contact: ContactDto): void {
    if (!confirm(`Delete ${contact.name}?`)) {
      return;
    }
    this.contacts.delete(contact.id).subscribe({
      next: () => this.items.update((list) => list.filter((c) => c.id !== contact.id)),
      error: () => this.error.set('Failed to delete contact.'),
    });
  }
}
