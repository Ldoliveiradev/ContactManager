import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { ContactList } from './contact-list/contact-list.component';
import { ContactForm } from './contact-form/contact-form.component';

type View = 'list' | 'new' | 'edit';

@Component({
  selector: 'app-contacts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ContactList, ContactForm],
  templateUrl: './contacts.component.html',
  styleUrl: './contacts.component.scss',
})
export class ContactsComponent {
  protected readonly view = signal<View>('list');
  protected readonly editId = signal<string | null>(null);

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
  }
}
