import {
  ChangeDetectionStrategy,
  Component,
  TemplateRef,
  computed,
  contentChild,
  contentChildren,
  effect,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgTemplateOutlet } from '@angular/common';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faMagnifyingGlass, faSort } from '@fortawesome/free-solid-svg-icons';
import { PaginationComponent } from '../pagination/pagination.component';
import { DataViewActionsDirective } from './data-view-actions.directive';
import { DataViewCellDirective } from './data-view-cell.directive';
import { DataViewColumn, SortDirection } from './data-view.types';

/**
 * Generic, enterprise-style data grid (card layout). Driven by column definitions;
 * supports built-in search across searchable columns, sortable column headers, and
 * custom cell / row-action templates via the [uiDataViewCell] and [uiDataViewActions]
 * directives. Renders rows as responsive cards rather than a desktop-only table.
 */
@Component({
  selector: 'ui-data-view',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, NgTemplateOutlet, FaIconComponent, PaginationComponent],
  templateUrl: './data-view.component.html',
  styleUrl: './data-view.component.scss',
})
export class DataViewComponent<T extends object> {
  protected readonly faSearch = faMagnifyingGlass;
  protected readonly faSort = faSort;

  readonly items = input.required<T[]>();
  readonly columns = input.required<DataViewColumn<T>[]>();
  readonly searchPlaceholder = input('Search…');
  readonly emptyMessage = input('No results.');
  /** Initial rows per page; 0 disables pagination. */
  readonly pageSize = input(0);
  /** Selectable page sizes shown in the pagination control (empty hides the selector). */
  readonly pageSizeOptions = input<number[]>([]);
  /** Pre-select an active sort column on first render (must match a column key). */
  readonly initialSortKey = input<(keyof T & string) | null>(null);
  /** Initial sort direction when initialSortKey is set (default 'asc'). */
  readonly initialSortDir = input<SortDirection>('asc');
  /**
   * Server-supplied total item count. When > 0, the component operates in
   * server-side pagination mode and uses this value for page calculations
   * instead of the length of the locally-filtered set.
   */
  readonly totalItems = input(0);
  /** In server-side mode, the parent drives the current page via this input. */
  readonly activePage = input(1);
  /** When true, shows a loading overlay without destroying the component. */
  readonly loading = input(false);

  readonly pageChange = output<number>();
  readonly pageSizeChange = output<number>();
  readonly searchChange = output<string>();

  private readonly cellTemplates = contentChildren(DataViewCellDirective);
  private readonly actionsDir = contentChild(DataViewActionsDirective);

  protected readonly query = signal('');
  protected readonly sortKey = signal<(keyof T & string) | null>(null);
  protected readonly sortDir = signal<SortDirection>('asc');
  protected readonly page = signal(1);
  // Effective page size: seeded from the input, mutable via the selector.
  protected readonly currentPageSize = signal(0);

  /** Exposes the current page for parent components that need to track it. */
  readonly currentPage = this.page.asReadonly();
  /** Exposes the effective page size for parent components that need to track it. */
  readonly effectivePageSize = this.currentPageSize.asReadonly();

  constructor() {
    // Seed the effective page size from the input.
    effect(() => this.currentPageSize.set(this.pageSize()));

    // Seed initial sort from inputs (runs once on first signal read).
    effect(() => {
      const key = this.initialSortKey();
      const dir = this.initialSortDir();
      if (key) {
        this.sortKey.set(key);
        this.sortDir.set(dir);
      }
    });

    // Client-side mode only: reset to page 1 on query, page-size, or item changes.
    // In server-side mode the parent owns the page via activePage(); don't touch it.
    effect(() => {
      this.query();
      this.currentPageSize();
      this.items();
      if (this.totalItems() === 0) {
        this.page.set(1);
      }
    });
  }

  protected get actionsTemplate(): TemplateRef<unknown> | null {
    return this.actionsDir()?.template ?? null;
  }

  protected readonly searchableColumns = computed(() =>
    this.columns().filter((c) => c.searchable),
  );

  /** Full filtered + sorted result set (before pagination). */
  protected readonly filtered = computed(() => {
    let rows = this.items();

    // In server mode the API handles filtering — skip client-side search.
    if (!this.serverMode()) {
      const q = this.query().trim().toLowerCase();
      const cols = this.searchableColumns();
      if (q && cols.length) {
        rows = rows.filter((row) =>
          cols.some((c) => String(row[c.key] ?? '').toLowerCase().includes(q)),
        );
      } else {
        rows = [...rows];
      }
    } else {
      rows = [...rows];
    }

    const key = this.sortKey();
    if (key) {
      const dir = this.sortDir() === 'asc' ? 1 : -1;
      rows.sort((a, b) =>
        String(a[key] ?? '').localeCompare(String(b[key] ?? '')) * dir,
      );
    }
    return rows;
  });

  protected readonly serverMode = computed(() => this.totalItems() > 0);

  protected readonly filteredCount = computed(() =>
    this.serverMode() ? this.totalItems() : this.filtered().length,
  );

  protected readonly effectivePage = computed(() =>
    this.serverMode() ? this.activePage() : this.page(),
  );

  /** The slice of rows shown on the current page (or all rows if paging is off). */
  protected readonly visible = computed(() => {
    const size = this.currentPageSize();
    if (size <= 0 || this.serverMode()) return this.filtered();
    const start = (this.effectivePage() - 1) * size;
    return this.filtered().slice(start, start + size);
  });

  protected cellTemplateFor(col: DataViewColumn<T>): TemplateRef<unknown> | null {
    if (!col.cell) {
      return null;
    }
    return this.cellTemplates().find((t) => t.uiDataViewCell() === col.cell)?.template ?? null;
  }

  protected setQuery(value: string): void {
    this.query.set(value);
    if (this.serverMode()) this.searchChange.emit(value);
  }

  protected setPage(page: number): void {
    if (!this.serverMode()) this.page.set(page);
    this.pageChange.emit(page);
  }

  protected setPageSize(size: number): void {
    this.currentPageSize.set(size);
    this.pageSizeChange.emit(size);
  }

  protected sortBy(col: DataViewColumn<T>): void {
    if (!col.sortable) {
      return;
    }
    if (this.sortKey() === col.key) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortKey.set(col.key);
      this.sortDir.set('asc');
    }
  }

  protected sortIndicator(col: DataViewColumn<T>): string {
    if (this.sortKey() !== col.key) {
      return '';
    }
    return this.sortDir() === 'asc' ? '▲' : '▼';
  }

  protected hasSearch = computed(() => this.searchableColumns().length > 0);
}
