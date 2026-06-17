import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { ContactsComponent } from './contacts.component';

describe('ContactsComponent', () => {
  let fixture: ComponentFixture<ContactsComponent>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContactsComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(ContactsComponent);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should create', () => {
    fixture.detectChanges();
    http.expectOne((r) => r.url === `${environment.apiUrl}/contacts`).flush({
      isSuccess: true, error: null, totalCount: 0, page: 1, pageSize: 10,
      totalPages: 0, hasPreviousPage: false, hasNextPage: false,
      data: [],
    });
    expect(fixture.componentInstance).toBeTruthy();
  });
});
