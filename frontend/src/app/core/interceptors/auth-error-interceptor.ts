import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { catchError, of, throwError } from "rxjs";
import { AuthService } from "../../features/auth/auth-service";
import { inject } from "@angular/core";
import { Router } from "@angular/router";

export const AuthErrorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService: AuthService = inject(AuthService);
    const router: Router = inject(Router);
    return next(req).pipe(
        catchError((err: HttpErrorResponse) => {
            if (err.status === 401 && !req.url.includes('refresh') && !req.url.includes('login')) {
                return authService.handleExpiredToken(req, next);
            }
            if (err.status === 403) {
                router.navigateByUrl('/systemPages')
            }
            // TODO: i think we should remove the handleApiError on individual api call just handle it all here???
            return throwError(() => err);
        })
    );
}