import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService, formatValidationErrors } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError(err => {
      if (err.status === 401) {
        auth.logout();
        router.navigate(['/login']);
      } else if (err.status === 400) {
        const body = err.error;
        if (body?.type === 'validation_error' && body?.errors) {
          const message = formatValidationErrors(body.errors as Record<string, string[]>);
          if (message) toast.show(message, 'error');
        }
      }
      return throwError(() => err);
    })
  );
};
