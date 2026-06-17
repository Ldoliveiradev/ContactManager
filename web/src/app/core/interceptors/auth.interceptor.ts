import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../features/auth/services/auth.service';

/**
 * Attaches the JWT bearer token to API requests and, on a 401, logs the user out
 * and redirects to login (token expired or invalid).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.getToken();
  const isApiRequest = req.url.startsWith(environment.apiUrl);
  const isAuthLoginRequest = req.url === `${environment.apiUrl}/auth/login`;

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err) => {
      if (!isApiRequest) {
        return throwError(() => err);
      }

      if (err.status === 401 && token && !isAuthLoginRequest) {
        auth.logout();
        router.navigate(['/401']);
      } else if (err.status === 404) {
        router.navigate(['/404']);
      } else if (err.status === 0 || err.status >= 500) {
        router.navigate(['/error']);
      }
      return throwError(() => err);
    }),
  );
};
