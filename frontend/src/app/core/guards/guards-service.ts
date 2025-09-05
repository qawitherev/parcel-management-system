import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';

interface JwtExp {
  exp: number
}

@Injectable({
  providedIn: 'root'
})
export class GuardsService {
  
  constructor(private http: HttpClient) {}

  isLoggedIn(): boolean {
    const token = localStorage.getItem(`parcel-management-system-token`)
    if (!token) {
      return false
    }
    const decoded = jwtDecode<JwtExp>(token);
    const now = Math.floor(Date.now() / 1000) // -> because exp is in second since epoch
    // and Date.now() return mili since epoch, thats why divide 1000
    return now <= decoded.exp // -> so we compare if now bigger than exp
  }
}
