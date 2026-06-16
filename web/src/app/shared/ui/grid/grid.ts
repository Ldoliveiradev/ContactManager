import {
  ChangeDetectionStrategy,
  Component,
  TemplateRef,
  computed,
  contentChild,
  contentChildren,
  effect,
  input,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgTemplateOutlet } from '@angular/common';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faMagnifyingGlass, faSort } from '@fortawesome/free-solid-svg-icons';
import { Pagination } from '../pagination/pagination';
import { GridActionsDirective } from './grid-actions.directive';
import { GridCellDirective } from './grid-cell.directive';
import { GridColumn, SortDirection } from './grid.types';

/**
 * Generic, enterprise-style data grid (card layout). Driven by column definitions;
 * supports built-in search across searchable columns, sortable column headers, and
 * custom cell / row-action templates via the [uiGridCell] and [uiGridActions]
 * directives. Renders rows as responsive cards rather than a desktop-only table.
 */
@Component({
  selector: 'ui-grid',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, NgTemplateOutlet, FaIconComponent, Pagination],
  templateUrl: './grid.html',
  styleUrl: './grid.scss',
})
export class Grid<T extends object> {
  protected readonly faSearch = faMagnifyingGlass;
  protected readonly faSort = faSort;

  readonly items = input.required<T[]>();
  readonly columns = input.required<GridColumn<T>[]>();
  readonly searchPlaceholder = input('Search…');
  readonly emptyMessage = input('No results.');
  /** Rows per page; 0 disables pagination. */
  readonly pageSize = input(0);

  private readonly cellTemplates = contentChildren(GridCellDirective);
  private readonly actionsDir = contentChild(GridActionsDirective);

  protected readonly query = signal('');
  protected readonly sortKey = signal<(keyof T & string) | null>(null);
  protected readonly sortDir = signal<SortDirection>('asc');
  protected readonly page = signal(1);

  constructor() {
    // Reset to the first page whenever the filtered result set changes.
    effect(() => {
      this.query();
      this.items();
      this.page.set(1);
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
    const q = this.query().trim().toLowerCase();
    const cols = this.searchableColumns();

    let rows = this.items();
    if (q && cols.length) {
      rows = rows.filter((row) =>
        cols.some((c) => String(row[c.key] ?? '').toLowerCase().includes(q)),
      );
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

  protected readonly filteredCount = computed(() => this.filtered().length);

  /** The slice of rows shown on the current page (or all rows if paging is off). */
  protected readonly visible = computed(() => {
    const size = this.pageSize();
    if (size <= 0) {
      return this.filtered();
    }
    const start = (this.page() - 1) * size;
    return this.filtered().slice(start, start + size);
  });

  protected cellTemplateFor(col: GridColumn<T>): TemplateRef<unknown> | null {
    if (!col.cell) {
      return null;
    }
    return this.cellTemplates().find((t) => t.uiGridCell() === col.cell)?.template ?? null;
  }

  protected setQuery(value: string): void {
    this.query.set(value);
  }

  protected setPage(page: number): void {
    this.page.set(page);
  }

  protected sortBy(col: GridColumn<T>): void {
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

  protected sortIndicator(col: GridColumn<T>): string {
    if (this.sortKey() !== col.key) {
      return '';
    }
    return this.sortDir() === 'asc' ? '▲' : '▼';
  }

  protected hasSearch = computed(() => this.searchableColumns().length > 0);
}
