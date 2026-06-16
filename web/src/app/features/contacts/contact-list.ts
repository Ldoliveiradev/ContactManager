import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Contact } from '../../core/models/contact.model';
import { ContactService } from '../../core/services/contact.service';

@Component({
  selector: 'app-contact-list',
  imports: [RouterLink],
  templateUrl: './contact-list.html',
  styleUrl: './contact-list.scss',
})
export class ContactList implements OnInit {
  private readonly contacts = inject(ContactService);

  protected readonly items = signal<Contact[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

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
