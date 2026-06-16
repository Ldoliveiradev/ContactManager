import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Pagination } from './pagination';

@Component({
  imports: [Pagination],
  template: `
    <ui-pagination
      [page]="page()"
      [pageSize]="pageSize"
      [totalItems]="total"
      [pageSizeOptions]="options"
      (pageChange)="onChange($event)"
      (pageSizeChange)="onSizeChange($event)"
    />
  `,
})
class Host {
  page = signal(1);
  pageSize = 10;
  total = 35;
  options: number[] = [];
  changed: number | null = null;
  sizeChanged: number | null = null;
  onChange(p: number) {
    this.changed = p;
  }
  onSizeChange(s: number) {
    this.sizeChanged = s;
  }
}

describe('Pagination', () => {
  let fixture: ComponentFixture<Host>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
  });

  function buttons(): HTMLButtonElement[] {
    return Array.from(fixture.nativeElement.querySelectorAll('.pagination__btn'));
  }

  it('renders a windowed set of page buttons (page 1 shows 1–3)', () => {
    const labels = buttons().map((b) => b.textContent?.trim());
    expect(labels).toContain('1');
    expect(labels).toContain('3');
  });

  it('shows the range label', () => {
    const range = fixture.nativeElement.querySelector('.pagination__range');
    expect(range.textContent.trim()).toBe('1–10 of 35');
  });

  it('emits pageChange when a page is clicked', () => {
    const page2 = buttons().find((b) => b.textContent?.trim() === '2');
    page2!.click();
    expect(fixture.componentInstance.changed).toBe(2);
  });

  it('does not render when there is a single page and no size options', () => {
    fixture.componentInstance.total = 5; // 5/10 → 1 page
    fixture.componentInstance.page.set(1);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.pagination')).toBeNull();
  });

  it('renders the page-size selector when options are provided', () => {
    fixture.componentInstance.options = [10, 20, 50];
    fixture.detectChanges();
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('.page-size__select');
    expect(select).toBeTruthy();
    expect(select.querySelectorAll('option').length).toBe(3);
  });

  it('emits pageSizeChange when a new size is selected', () => {
    fixture.componentInstance.options = [10, 20, 50];
    fixture.detectChanges();
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('.page-size__select');
    select.value = select.options[2].value; // 50
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    expect(fixture.componentInstance.sizeChanged).toBe(50);
  });

  it('shows the size selector even with a single page', () => {
    fixture.componentInstance.total = 5; // 1 page
    fixture.componentInstance.options = [10, 20];
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.pagination')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.page-size__select')).toBeTruthy();
  });
});
