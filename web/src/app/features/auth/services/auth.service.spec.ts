import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('isAuthenticated is false when no token in storage', () => {
    expect(service.isAuthenticated()).toBeFalse();
  });

  it('login stores token and sets isAuthenticated', () => {
    service.login({ username: 'demo', password: 'Secret123!' }).subscribe();

    const req = http.expectOne(`${environment.apiUrl}/auth/login`);
    req.flush({ isSuccess: true, error: null, data: { token: 'jwt-abc' } });

    expect(service.isAuthenticated()).toBeTrue();
    expect(localStorage.getItem('cm_token')).toBe('jwt-abc');
  });

  it('login does not store token when isSuccess is false', () => {
    service.login({ username: 'demo', password: 'wrong' }).subscribe();

    const req = http.expectOne(`${environment.apiUrl}/auth/login`);
    req.flush({ isSuccess: false, error: 'Invalid credentials', data: null });

    expect(service.isAuthenticated()).toBeFalse();
  });

  it('logout clears token and sets isAuthenticated to false', () => {
    service.login({ username: 'demo', password: 'Secret123!' }).subscribe();
    http.expectOne(`${environment.apiUrl}/auth/login`).flush({
      isSuccess: true, error: null, data: { token: 'jwt-abc' },
    });

    service.logout();

    expect(service.isAuthenticated()).toBeFalse();
    expect(localStorage.getItem('cm_token')).toBeNull();
  });

  it('register sends correct payload', () => {
    const payload = {
      username: 'demo', firstName: 'Jane', lastName: 'Doe',
      email: 'jane@example.com', password: 'Secret123!',
    };
    service.register(payload).subscribe();

    const req = http.expectOne(`${environment.apiUrl}/auth/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ isSuccess: true, error: null, data: { id: '1', username: 'demo', fullName: 'Jane Doe' } });
  });
});
