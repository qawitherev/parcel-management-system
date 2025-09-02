import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of, tap } from 'rxjs';
import { AuthEndpoints } from '../core/endpoints/auth-endpoints';

interface RegisterRequest {
  Username: string, 
  Email: string, 
  ResidentUnit: string, 
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

  
  constructor(private http: HttpClient) {
    // do nothing 
  }
  // TODO: create a console utility (the one that we can turn on/off)
  register(registerPayload: RegisterRequest): Observable<any> {
    console.info(`Sending request: ${JSON.stringify(registerPayload)}`)
    return this.http.post<RegisterResponse>(AuthEndpoints.register, registerPayload)
      .pipe(
        catchError(err => {
        console.error(err)
        return of({ error: true, message: err.error.message || 'Unknown error' });
      })
      )
  } 

  login(loginPayload: LoginRequest): Observable<any> {
    console.info(`Sending request: ${JSON.stringify(loginPayload)}`)
    return this.http.post<LoginResponse>(AuthEndpoints.login, loginPayload)
      .pipe(
        tap(res => {
          this.saveToken(res.token)
        }),
        catchError(err => {
          console.error(err)
          return of({error: true, message: err.error.message || 'Unknown error'})
        })
      )
  }

  saveToken(token: string) {
    localStorage.setItem('parcel-management-system-token', token);
  }
}
