import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faChevronLeft, faChevronRight } from '@fortawesome/free-solid-svg-icons';

/**
 * Presentational pagination control. The parent owns the page state and reacts to
 * the `pageChange` output; this component only renders controls and emits intent.
 */
@Component({
  selector: 'ui-pagination',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FaIconComponent],
  templateUrl: './pagination.html',
  styleUrl: './pagination.scss',
})
export class Pagination {
  readonly page = input.required<number>(); // 1-based
  readonly pageSize = input.required<number>();
  readonly totalItems = input.required<number>();

  readonly pageChange = output<number>();

  protected readonly faPrev = faChevronLeft;
  protected readonly faNext = faChevronRight;

  protected readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalItems() / this.pageSize())),
  );

  protected readonly canPrev = computed(() => this.page() > 1);
  protected readonly canNext = computed(() => this.page() < this.totalPages());

  /** Page numbers to render (a small window around the current page). */
  protected readonly pages = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    const window = 2;
    const start = Math.max(1, current - window);
    const end = Math.min(total, current + window);
    const result: number[] = [];
    for (let i = start; i <= end; i++) {
      result.push(i);
    }
    return result;
  });

  protected readonly rangeLabel = computed(() => {
    const total = this.totalItems();
    if (total === 0) {
      return '0 of 0';
    }
    const from = (this.page() - 1) * this.pageSize() + 1;
    const to = Math.min(this.page() * this.pageSize(), total);
    return `${from}–${to} of ${total}`;
  });

  protected go(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.page()) {
      this.pageChange.emit(page);
    }
  }
}
