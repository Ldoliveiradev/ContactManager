import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { ContactDto } from '../models/contact-dto.interface';
import { ContactService } from './contact.service';

const baseUrl = `${environment.apiUrl}/contacts`;

const mockContact: ContactDto = { id: '1', name: 'Jane Doe', email: 'jane@example.com', phone: null };

describe('ContactService', () => {
  let service: ContactService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ContactService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getAll unwraps PaginationResponse data list', () => {
    let result: ContactDto[] = [];
    service.getAll().subscribe((contacts) => (result = contacts));

    const req = http.expectOne((r) => r.url === baseUrl && r.method === 'GET');
    expect(req.request.params.get('PageSize')).toBe('1000');
    req.flush({
      isSuccess: true, error: null, totalCount: 1, page: 1, pageSize: 10,
      totalPages: 1, hasPreviousPage: false, hasNextPage: false,
      data: { isSuccess: true, error: null, data: [mockContact] },
    });

    expect(result).toEqual([mockContact]);
  });

  it('getAll returns empty array when data is null', () => {
    let result: ContactDto[] = [];
    service.getAll().subscribe((contacts) => (result = contacts));

    http.expectOne((r) => r.url === baseUrl && r.method === 'GET').flush({
      isSuccess: false, error: 'error', data: null,
      totalCount: 0, page: 1, pageSize: 10, totalPages: 0,
      hasPreviousPage: false, hasNextPage: false,
    });

    expect(result).toEqual([]);
  });

  it('getById unwraps ContactResponse data', () => {
    let result: ContactDto | null = null;
    service.getById('1').subscribe((c) => (result = c));

    http.expectOne(`${baseUrl}/1`).flush({ isSuccess: true, error: null, data: mockContact });

    expect<ContactDto | null>(result).toEqual(mockContact);
  });

  it('create sends POST and unwraps data', () => {
    const input = { name: 'Jane', email: 'jane@example.com', phone: null };
    let result: ContactDto | null = null;
    service.create(input).subscribe((c) => (result = c));

    const req = http.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(input);
    req.flush({ isSuccess: true, error: null, data: mockContact });

    expect<ContactDto | null>(result).toEqual(mockContact);
  });

  it('update sends PUT and unwraps data', () => {
    const input = { name: 'Jane Updated', email: 'jane@example.com', phone: null };
    let result: ContactDto | null = null;
    service.update('1', input).subscribe((c) => (result = c));

    const req = http.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('PUT');
    req.flush({ isSuccess: true, error: null, data: { ...mockContact, name: 'Jane Updated' } });

    expect((result as ContactDto | null)?.name).toBe('Jane Updated');
  });

  it('delete sends DELETE request', () => {
    service.delete('1').subscribe();

    const req = http.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
