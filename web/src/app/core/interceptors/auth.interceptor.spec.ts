import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../../features/auth/services/auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let auth: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  function setup(token: string | null) {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['getToken', 'logout']);
    auth.getToken.and.returnValue(token);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
        { provide: Router, useValue: router },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  }

  afterEach(() => httpMock.verify());

  it('attaches the bearer token when present', () => {
    setup('jwt-123');
    http.get(`${environment.apiUrl}/contacts`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/contacts`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer jwt-123');
    req.flush([]);
  });

  it('does not attach Authorization when there is no token', () => {
    setup(null);
    http.get(`${environment.apiUrl}/contacts`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/contacts`);
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush([]);
  });

  it('logs out and redirects to /login on a 401', () => {
    setup('jwt-123');
    http.get(`${environment.apiUrl}/contacts`).subscribe({ error: () => {} });

    const req = httpMock.expectOne(`${environment.apiUrl}/contacts`);
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(auth.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/401']);
  });

  it('redirects to /404 on an API 404', () => {
    setup('jwt-123');
    http.get(`${environment.apiUrl}/contacts/missing`).subscribe({ error: () => {} });

    const req = httpMock.expectOne(`${environment.apiUrl}/contacts/missing`);
    req.flush('Not found', { status: 404, statusText: 'Not Found' });

    expect(router.navigate).toHaveBeenCalledWith(['/404']);
  });

  it('redirects to /error on an API 500', () => {
    setup('jwt-123');
    http.get(`${environment.apiUrl}/contacts`).subscribe({ error: () => {} });

    const req = httpMock.expectOne(`${environment.apiUrl}/contacts`);
    req.flush('Server error', { status: 500, statusText: 'Server Error' });

    expect(router.navigate).toHaveBeenCalledWith(['/error']);
  });

  it('does not redirect login failures to /401', () => {
    setup(null);
    http.post(`${environment.apiUrl}/auth/login`, { username: 'demo', password: 'wrong' }).subscribe({ error: () => {} });

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(auth.logout).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
