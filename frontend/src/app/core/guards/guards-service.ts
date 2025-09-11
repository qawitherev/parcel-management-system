import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { map, Observable, of } from 'rxjs';
import { RoleService } from '../roles/role-service';

interface JwtExp {
  exp: number;
}

@Injectable({
  providedIn: 'root',
})
export class GuardsService {
  private _cachedRole: string | null = null;
  constructor(private http: HttpClient, private roleService: RoleService) {}

  isLoggedIn(): boolean {
    const token = localStorage.getItem(`parcel-management-system-token`);
    if (!token) {
      return false;
    }
    var decoded: any
    try {
      decoded = jwtDecode<JwtExp>(token);
    } catch {
      return false
    }
    const now = Math.floor(Date.now() / 1000); // -> because exp is in second since epoch
    // and Date.now() return mili since epoch, thats why divide 1000
    return now <= decoded.exp; // -> so we compare if now bigger than exp
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
