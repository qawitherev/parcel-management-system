import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, filter, Observable, of, switchMap, take, tap, throwError } from 'rxjs';
import { AuthEndpoints } from '../../core/endpoints/auth-endpoints';
import { AppConsole } from '../../utils/app-console';
import { Register } from './pages/register/register';
import { Router } from '@angular/router';
import { handleApiError } from '../../core/error-handling/api-catch-error';
import { RoleService } from '../../core/roles/role-service';

interface RegisterResidentRequest {
  Username: string, 
  Email: string, 
  ResidentUnit: string, 
  Password: string
}

interface RegisterManagerRequest {
  Username: string, 
  Email: string, 
  Password: string
}

interface RegisterResponse {
  UserId: string, 
  Username: string
}

interface LoginRequest {
  Username: string, 
  PlainPassword: string
}

interface LoginResponse {
  accessToken: string
}

export const TOKEN_STORAGE_KEY = "parcel-management-system-token"

@Injectable({
  providedIn: 'root' // --> means this is a singleton
})
export class AuthService {
  isRefreshing: boolean = false;
  refreshTokenSubject = new BehaviorSubject<string | null>(null);
  constructor(
    private roleService: RoleService,
    private http: HttpClient, private router: Router,
  ) {}

  register(registerPayload: RegisterResidentRequest): Observable<any> {
    AppConsole.log(`Sending request: ${JSON.stringify(registerPayload)}`)
    return this.http.post<RegisterResponse>(AuthEndpoints.register, registerPayload)
      .pipe(
        catchError(handleApiError)
      )
  } 

  login(loginPayload: LoginRequest): Observable<any> {
    AppConsole.log(`Sending request: ${JSON.stringify(loginPayload)}`)
    return this.http.post<LoginResponse>(AuthEndpoints.login, loginPayload)
      .pipe(
        tap(res => {
          this.saveToken(res.accessToken)
        }),
        catchError(handleApiError)
      )
  }

  registerManager(registerRequest: RegisterManagerRequest) : Observable<any>{
    AppConsole.log(`Sending request register manager: ${JSON.stringify(registerRequest)}`)
    return this.http.post(AuthEndpoints.registerManager, registerRequest)
      .pipe(
        catchError(handleApiError), 
        tap(_ => {
          this.router.navigateByUrl('/login')
        })
      )
  }

  logout() {
    this.roleService.clearRole()
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    this.router.navigateByUrl('/login');
  }

  saveToken(token: string) {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
  }

  refreshToken(): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(AuthEndpoints.refreshToken, "");
  }

  handleExpiredToken(req: HttpRequest<any>, next: any): Observable<any> {
    if (!this.isRefreshing) {
      this.isRefreshing = true; 
      this.refreshTokenSubject.next(null);
      return this.refreshToken().pipe(
        switchMap(res => {
          this.isRefreshing = false;
          this.saveToken(res.accessToken);
          this.refreshTokenSubject.next(res.accessToken);
          return next(req.clone( {
            headers: req.headers.set('Authorization', `Bearer ${res.accessToken}`)
          })); 
        }), 
        catchError(() => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(null);
          this.logout();
          this.router.navigateByUrl('/login');
          return of();
        })
      )
    } else {
      return this.refreshTokenSubject.pipe(
        filter(token => token != null), 
        take(1), 
        switchMap((token) => {
          return next(req.clone({
            headers: req.headers.set('Authorization', `Bearer ${token}`)
          }))
        })
      );
    }
  }

  
}
