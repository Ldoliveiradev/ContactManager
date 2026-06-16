import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { AlertComponent } from '../../../shared/ui/alert/alert.component';
import { ButtonComponent } from '../../../shared/ui/button/button.component';
import { SkeletonComponent } from '../../../shared/ui/skeleton/skeleton.component';
import { ContactDto } from '../models/contact-dto.interface';
import { ContactService } from '../services/contact.service';
import { ContactForm } from './contact-form/contact-form.component';
import { ContactList } from './contact-list/contact-list.component';

type View = 'list' | 'new' | 'edit';

@Component({
  selector: 'app-contacts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ContactList, ContactForm, FaIconComponent, AlertComponent, ButtonComponent, SkeletonComponent],
  templateUrl: './contacts.component.html',
  styleUrl: './contacts.component.scss',
})
export class ContactsComponent implements OnInit {
  private readonly contactService = inject(ContactService);

  protected readonly view = signal<View>('list');
  protected readonly editId = signal<string | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly items = signal<ContactDto[]>([]);

  protected readonly faPlus = faPlus;
  protected readonly skeletonCards = [0, 1, 2, 3, 4, 5];

  ngOnInit(): void {
    this.loadContacts();
  }

  protected loadContacts(): void {
    this.loading.set(true);
    this.error.set(null);
    this.contactService.getAll().subscribe({
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

  protected showNew(): void {
    this.editId.set(null);
    this.view.set('new');
  }

  protected showEdit(id: string): void {
    this.editId.set(id);
    this.view.set('edit');
  }

  protected showList(): void {
    this.editId.set(null);
    this.view.set('list');
    this.loadContacts();
  }

  protected onDelete(id: string): void {
    this.items.update((list) => list.filter((c) => c.id !== id));
  }

  protected onDeleteError(msg: string): void {
    this.error.set(msg);
  }
}
