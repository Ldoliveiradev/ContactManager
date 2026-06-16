import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { ContactList } from './contact-list';

const mockPaginatedResponse = {
  isSuccess: true, error: null,
  totalCount: 1, page: 1, pageSize: 10, totalPages: 1,
  hasPreviousPage: false, hasNextPage: false,
  data: {
    isSuccess: true, error: null,
    data: [{ id: '1', name: 'Jane Doe', email: 'jane@example.com', phone: null }],
  },
};

describe('ContactList', () => {
  let fixture: ComponentFixture<ContactList>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContactList],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ContactList);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
    http.expectOne(`${environment.apiUrl}/contacts`).flush(mockPaginatedResponse);
  });

  it('renders contacts after load', () => {
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/contacts`).flush(mockPaginatedResponse);
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('Jane Doe');
  });

  it('shows empty state when no contacts', () => {
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/contacts`).flush({
      ...mockPaginatedResponse,
      totalCount: 0,
      data: { isSuccess: true, error: null, data: [] },
    });
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('No contacts yet');
  });
});
