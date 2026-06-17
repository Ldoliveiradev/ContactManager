import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../../features/auth/services/auth.service';

describe('authGuard', () => {
  function run(isAuthenticated: boolean) {
    const authStub = { isAuthenticated: () => isAuthenticated } as Partial<AuthService>;
    const urlTree = {} as UrlTree;
    const routerStub = { createUrlTree: jasmine.createSpy('createUrlTree').and.returnValue(urlTree) };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authStub },
        { provide: Router, useValue: routerStub },
      ],
    });

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    );
    return { result, routerStub, urlTree };
  }

  it('allows activation when authenticated', () => {
    const { result } = run(true);
    expect(result).toBeTrue();
  });

  it('redirects to /login when not authenticated', () => {
    const { result, routerStub, urlTree } = run(false);
    expect(routerStub.createUrlTree).toHaveBeenCalledWith(['/login']);
    expect(result).toBe(urlTree);
  });
});
