import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Grid } from './grid';
import { GridColumn } from './grid.types';

interface Row {
  name: string;
  email: string;
}

@Component({
  imports: [Grid],
  template: `
    <ui-grid
      [items]="items()"
      [columns]="columns"
      [pageSize]="pageSize"
      [pageSizeOptions]="pageSizeOptions"
    />
  `,
})
class Host {
  items = signal<Row[]>([
    { name: 'Charlie', email: 'c@x.com' },
    { name: 'Alice', email: 'a@x.com' },
    { name: 'Bob', email: 'b@x.com' },
  ]);
  columns: GridColumn<Row>[] = [
    { key: 'name', header: 'Name', sortable: true, searchable: true },
    { key: 'email', header: 'Email', searchable: true },
  ];
  pageSize = 0;
  pageSizeOptions: number[] = [];
}

describe('Grid', () => {
  let fixture: ComponentFixture<Host>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
  });

  function cardText(): string[] {
    return Array.from(fixture.nativeElement.querySelectorAll('.grid-card__value')).map(
      (el: any) => el.textContent.trim(),
    );
  }

  function search(value: string): void {
    const input: HTMLInputElement = fixture.nativeElement.querySelector('.search__input');
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('renders all rows by default', () => {
    expect(fixture.nativeElement.querySelectorAll('.grid-card').length).toBe(3);
  });

  it('filters rows by the search query', () => {
    search('alice');
    expect(fixture.nativeElement.querySelectorAll('.grid-card').length).toBe(1);
    expect(cardText()).toContain('Alice');
  });

  it('shows the empty message when nothing matches', () => {
    search('zzz');
    expect(fixture.nativeElement.querySelector('.no-results')).toBeTruthy();
  });

  it('sorts ascending then descending when the sort button is toggled', () => {
    const sortBtn: HTMLButtonElement = fixture.nativeElement.querySelector('.sort__btn');
    sortBtn.click();
    fixture.detectChanges();
    // First column value of the first card should be the alphabetically-first name.
    expect(cardText()[0]).toBe('Alice');

    sortBtn.click();
    fixture.detectChanges();
    expect(cardText()[0]).toBe('Charlie');
  });

  it('paginates when pageSize is set', () => {
    fixture.componentInstance.pageSize = 2;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.grid-card').length).toBe(2);
    expect(fixture.nativeElement.querySelector('.pagination')).toBeTruthy();
  });

  it('changing page size via the selector re-slices the rows', () => {
    fixture.componentInstance.pageSize = 2;
    fixture.componentInstance.pageSizeOptions = [2, 5];
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.grid-card').length).toBe(2);

    const select: HTMLSelectElement = fixture.nativeElement.querySelector('.page-size__select');
    select.value = select.options[1].value; // 5
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    // All 3 rows now fit on one page.
    expect(fixture.nativeElement.querySelectorAll('.grid-card').length).toBe(3);
  });
});
