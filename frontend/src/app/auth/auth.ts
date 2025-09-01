import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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
  EmailUsername: string, 
  Password: string
}

@Injectable({
  providedIn: 'root' // --> means this is a singleton
})
export class Auth {

  
  constructor(private http: HttpClient) {
    // do nothing 
  }

  register(registerPayload: RegisterRequest): Observable<any> {
    return this.http.post(AuthEndpoints.register, registerPayload)
  } 

  login(loginPayload: LoginRequest) {
    return this.http.post(AuthEndpoints.login, loginPayload)
  }  
}
