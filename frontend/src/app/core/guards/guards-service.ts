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
  constructor(private roleService: RoleService) {}

  isAccessTokenExist(): boolean {
    const token = localStorage.getItem(TOKEN_STORAGE_KEY);
    if (!token) {
      return false;
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
