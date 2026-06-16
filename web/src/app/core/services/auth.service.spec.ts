import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiUrl}/auth`;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('starts unauthenticated when no token is stored', () => {
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.getToken()).toBeNull();
  });

  it('stores the token and becomes authenticated after login', () => {
    service.login({ username: 'demo', password: 'Secret123!' }).subscribe();

    const req = httpMock.expectOne(`${base}/login`);
    expect(req.request.method).toBe('POST');
    req.flush({ token: 'jwt-123' });

    expect(service.isAuthenticated()).toBeTrue();
    expect(service.getToken()).toBe('jwt-123');
    expect(localStorage.getItem('cm_token')).toBe('jwt-123');
  });

  it('register posts credentials and does not authenticate by itself', () => {
    service.register({ username: 'new', password: 'Secret123!' }).subscribe();

    const req = httpMock.expectOne(`${base}/register`);
    expect(req.request.method).toBe('POST');
    req.flush({ id: 'abc', username: 'new' });

    expect(service.isAuthenticated()).toBeFalse();
  });

  it('logout clears the token and storage', () => {
    service.login({ username: 'demo', password: 'Secret123!' }).subscribe();
    httpMock.expectOne(`${base}/login`).flush({ token: 'jwt-123' });

    service.logout();

    expect(service.isAuthenticated()).toBeFalse();
    expect(localStorage.getItem('cm_token')).toBeNull();
  });

  it('restores auth state from a previously stored token', () => {
    localStorage.setItem('cm_token', 'persisted-token');
    // New instance reads localStorage on construction.
    const fresh = TestBed.runInInjectionContext(() => new AuthService());
    expect(fresh.isAuthenticated()).toBeTrue();
    expect(fresh.getToken()).toBe('persisted-token');
  });
});
