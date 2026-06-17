import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { AlertComponent } from '../../../shared/ui/alert/alert.component';
import { ButtonComponent } from '../../../shared/ui/button/button.component';
import { SkeletonComponent } from '../../../shared/ui/skeleton/skeleton.component';
import { ContactDto } from '../models/contact-dto.interface';
import { ToastService } from '../../../core/services/toast.service';
import { ContactService } from '../services/contact.service';
import { ContactList } from './contact-list/contact-list.component';

@Component({
  selector: 'app-contacts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ContactList, FaIconComponent, AlertComponent, ButtonComponent, SkeletonComponent],
  templateUrl: './contacts.component.html',
  styleUrl: './contacts.component.scss',
})
export class ContactsComponent implements OnInit, OnDestroy {
  private readonly contactService = inject(ContactService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly searchSubject = new Subject<string>();
  private readonly destroy$ = new Subject<void>();

  protected readonly loading = signal(true);
  protected readonly initialLoading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly items = signal<ContactDto[]>([]);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(6);
  protected readonly totalCount = signal(0);
  protected readonly search = signal('');

  protected readonly faPlus = faPlus;
  protected readonly skeletonCards = [0, 1, 2, 3, 4, 5];

  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$),
    ).subscribe((q) => {
      // Only fire the API when the user typed 3+ chars or cleared the field.
      if (q.length === 0 || q.length >= 3) {
        this.search.set(q);
        this.page.set(1);
        this.loadContacts();
      }
    });

    this.loadContacts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected loadContacts(): void {
    this.loading.set(true);
    this.error.set(null);
    this.contactService.getAll({
      page: this.page(),
      pageSize: this.pageSize(),
      search: this.search(),
    }).subscribe({
      next: (response) => {
        this.items.set(response.data ?? []);
        this.totalCount.set(response.totalCount);
        this.loading.set(false);
        this.initialLoading.set(false);
      },
      error: () => {
        this.error.set('Failed to load contacts.');
        this.loading.set(false);
        this.initialLoading.set(false);
      },
    });
  }

  protected onPageChange(page: number): void {
    this.page.set(page);
    this.loadContacts();
  }

  protected onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
    this.loadContacts();
  }

  protected onSearchChange(q: string): void {
    this.searchSubject.next(q.trim());
  }

  protected showNew(): void {
    this.router.navigate(['/contacts/new']);
  }

  protected showEdit(id: string): void {
    this.router.navigate(['/contacts', id, 'edit']);
  }

  protected onDelete(id: string): void {
    this.toast.show('Contact deleted.', 'success');
    // If the deleted item was the only one on this page, go back one page.
    const newTotal = this.totalCount() - 1;
    const maxPage = Math.max(1, Math.ceil(newTotal / this.pageSize()));
    if (this.page() > maxPage) this.page.set(maxPage);
    this.loadContacts();
  }

  protected onDeleteError(msg: string): void {
    this.toast.show(msg, 'error');
  }
}
