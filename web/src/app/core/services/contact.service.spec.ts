import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { Contact } from '../models/contact.model';
import { ContactService } from './contact.service';

describe('ContactService', () => {
  let service: ContactService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiUrl}/contacts`;

  const sample: Contact = { id: '1', name: 'Ada', email: 'ada@example.com', phone: null };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ContactService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ContactService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getAll issues GET to the contacts endpoint', () => {
    service.getAll().subscribe((list) => expect(list).toEqual([sample]));
    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('GET');
    req.flush([sample]);
  });

  it('getById issues GET with the id', () => {
    service.getById('1').subscribe((c) => expect(c).toEqual(sample));
    const req = httpMock.expectOne(`${base}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(sample);
  });

  it('create POSTs the payload', () => {
    const input = { name: 'Ada', email: 'ada@example.com', phone: null };
    service.create(input).subscribe((c) => expect(c).toEqual(sample));
    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(input);
    req.flush(sample);
  });

  it('update PUTs to the id', () => {
    const input = { name: 'Ada L.', email: 'ada.l@example.com', phone: '123' };
    service.update('1', input).subscribe();
    const req = httpMock.expectOne(`${base}/1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(input);
    req.flush({ ...sample, ...input });
  });

  it('delete DELETEs the id', () => {
    service.delete('1').subscribe();
    const req = httpMock.expectOne(`${base}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
