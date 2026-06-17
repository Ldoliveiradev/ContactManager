import { Component, inject, input, output } from '@angular/core';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faEnvelope,
  faPenToSquare,
  faPhone,
  faTrashCan,
} from '@fortawesome/free-solid-svg-icons';
import { PhonePipe } from '../../../../shared/pipes/phone.pipe';
import { DataViewComponent } from '../../../../shared/ui/data-view/data-view.component';
import { DataViewActionsDirective } from '../../../../shared/ui/data-view/data-view-actions.directive';
import { DataViewCellDirective } from '../../../../shared/ui/data-view/data-view-cell.directive';
import { DataViewColumn } from '../../../../shared/ui/data-view/data-view.types';
import { ContactDto } from '../../models/contact-dto.interface';
import { ContactService } from '../../services/contact.service';

@Component({
  selector: 'app-contact-list',
  imports: [
    PhonePipe,
    FaIconComponent,
    DataViewComponent,
    DataViewCellDirective,
    DataViewActionsDirective,
  ],
  templateUrl: './contact-list.component.html',
  styleUrl: './contact-list.component.scss',
})
export class ContactList {
  private readonly contactService = inject(ContactService);

  readonly items = input.required<ContactDto[]>();
  readonly loading = input(false);
  readonly totalCount = input(0);
  readonly activePage = input(1);
  readonly activePageSize = input(6);
  readonly editContact = output<string>();
  readonly deleted = output<string>();
  readonly deleteError = output<string>();
  readonly pageChange = output<number>();
  readonly pageSizeChange = output<number>();
  readonly searchChange = output<string>();

  protected readonly faEnvelope = faEnvelope;
  protected readonly faPhone = faPhone;
  protected readonly faPenToSquare = faPenToSquare;
  protected readonly faTrashCan = faTrashCan;

  protected readonly columns: DataViewColumn<ContactDto>[] = [
    { key: 'name', header: 'Name', sortable: true, searchable: true, cell: 'name' },
    { key: 'email', header: 'Email', sortable: true, searchable: true, cell: 'email' },
    { key: 'phone', header: 'Phone', searchable: true, cell: 'phone' },
  ];

  protected onEditContact(contact: ContactDto): void {
    this.editContact.emit(contact.id);
  }

  protected remove(contact: ContactDto): void {
    if (!confirm(`Delete ${contact.name}?`)) {
      return;
    }
    this.contactService.delete(contact.id).subscribe({
      next: () => this.deleted.emit(contact.id),
      error: () => this.deleteError.emit('Failed to delete contact.'),
    });
  }
}
