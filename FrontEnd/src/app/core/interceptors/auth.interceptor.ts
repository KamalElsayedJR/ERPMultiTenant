import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, switchMap, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';

const PUBLIC_ENDPOINTS = ['/auth/login', '/auth/register', '/auth/refresh', '/auth/refresh-token', '/auth/logout'];

const isPublicEndpoint = (url: string): boolean =>
  PUBLIC_ENDPOINTS.some((endpoint) => url.includes(endpoint));

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toastr = inject(ToastrService);

  const shouldSkip = isPublicEndpoint(req.url);
  let requestToHandle = req;

  if (!shouldSkip) {

    const currentToken = authService.getToken();

    if (currentToken) {
      requestToHandle = req.clone({
        setHeaders: {
          Authorization: `Bearer ${currentToken}`
        }
      });
    }
  }

  return next(requestToHandle).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isPublicEndpoint(req.url)) {
        return throwError(() => error);
      }
      if (req.url.includes('/auth/refresh-token')) {
        return throwError(() => error);
      }
      return authService.refreshAccessToken().pipe(
        switchMap((newToken) => {
          if (!newToken) {
            authService.forceLogout();
            toastr.error('Session expired. Please log in again.');
            router.navigate(['/login']);
            return throwError(() => error);
          }

          const retryRequest = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
          return next(retryRequest.clone({
            setHeaders: {
              Authorization: `Bearer ${newToken}`
            }
          }));
        }),
        catchError((refreshError) => {
          authService.forceLogout();
          toastr.error('Session expired. Please log in again.');
          router.navigate(['/login']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};
