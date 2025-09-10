import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of, tap } from 'rxjs';
import { AuthEndpoints } from '../../core/endpoints/auth-endpoints';
import { AppConsole } from '../../utils/app-console';
import { Register } from './pages/register/register';
import { Router } from '@angular/router';
import { handleApiError } from '../../core/error-handling/api-catch-error';

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
  token: string
}

@Injectable({
  providedIn: 'root' // --> means this is a singleton
})
export class Auth {

  
  constructor(private http: HttpClient, private router: Router) {}

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
          this.saveToken(res.token)
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

  saveToken(token: string) {
    localStorage.setItem('parcel-management-system-token', token);
  }



  
}
