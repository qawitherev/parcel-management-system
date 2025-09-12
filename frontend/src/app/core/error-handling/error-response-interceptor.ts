import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, of, throwError } from 'rxjs';
import { AppConsole } from '../../utils/app-console';

export const ErrorResponseInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((err) => {
        AppConsole.log(`interceptor error: ${JSON.stringify(err)}`)
      if (err.status === 401) {
        router.navigate(['/login'], {
          queryParams: { returnUrl: router.url },
        });
      } 
      return throwError(() => err);
    })
  );
};
