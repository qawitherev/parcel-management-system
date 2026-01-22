import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { catchError, throwError } from "rxjs";
import { AuthService } from "../../features/auth/auth-service";
import { inject } from "@angular/core";

export const AuthErrorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService: AuthService = inject(AuthService);
    return next(req).pipe(
        catchError((err: HttpErrorResponse) => {
            if (err.status == 401 && !req.url.includes('refresh') && !req.url.includes('login')) {
                return authService.handleExpiredToken(req, next);
            }
            return throwError(() => new Error(err.message));
        })
    );
}