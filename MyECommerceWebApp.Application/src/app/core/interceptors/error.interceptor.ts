import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ApiProblem } from '../models/ecommerce.models';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/api/auth/')) {
        auth.logout();
      }

      const problem = error.error as ApiProblem | string | null;
      const detail =
        typeof problem === 'string'
          ? problem
          : problem?.detail || problem?.title || error.message || 'Error de red';

      return throwError(() => ({ status: error.status, message: detail, raw: error }));
    })
  );
};
