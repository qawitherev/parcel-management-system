import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { catchError, map, Observable, of } from 'rxjs';
import { RoleService } from '../roles/role-service';
import { AuthService, TOKEN_STORAGE_KEY } from '../../features/auth/auth-service';
import { handleApiError } from '../error-handling/api-catch-error';

interface JwtExp {
  exp: number;
}

@Injectable({
  providedIn: 'root',
})
export class GuardsService {
  private _cachedRole: string | null = null;
  constructor(private http: HttpClient, private roleService: RoleService, private authService: AuthService) {}

  isLoggedIn(): boolean {
    const token = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!token) {
      return false;
    }
    var decoded: any
    try {
      decoded = jwtDecode<JwtExp>(token);
    } catch {
      return false
    }
    const now = Math.floor(Date.now() / 1000); 
    if (now <= decoded.exp) {
      const newAccessToken = this.authService.refreshToken().pipe(
        map(response => {
          if (response && 'accessToken' in response) {
            localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken); 
            return true;
          }
          return false;
        }), 
        catchError(() => {
          localStorage.removeItem(TOKEN_STORAGE_KEY);
          return of(false);
        })
      )
    }
    return true;
  }

  isRoleAuthorized$(roles: string[]): Observable<boolean> {
    return this.roleService.getRole().pipe(
      map((res) => {
        if (!res) {
          return false
        }
        return roles.includes(res.role)
      })
    );
  }
}
