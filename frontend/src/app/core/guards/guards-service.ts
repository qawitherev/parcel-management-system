import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../../environment/environment';
import { map, of } from 'rxjs';
import { AppConsole } from '../../utils/app-console';

interface JwtExp {
  exp: number
}

interface UserRole {
  role: string
}

@Injectable({
  providedIn: 'root'
})
export class GuardsService {
  private cachedRole: string |null = null
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

  isRoleAuthorized$(roles: string[]) {
    if (this.cachedRole) {
      return of(roles.includes(this.cachedRole))
    }
    const storedRole = sessionStorage.getItem(`parcel-management-system-role`)
    if (storedRole && roles.includes(storedRole)) {
      AppConsole.log(`Stored role is ${storedRole}`)
      this.cachedRole = storedRole
      return of(roles.includes(this.cachedRole))
    }
    AppConsole.log(`Checking for cached failed, fallback api`)
    return this.http.get<UserRole>(`${environment.apiBaseUrl}/user/basic`).pipe(
      map(res => {
        AppConsole.log(`api/user/basic: ${JSON.stringify(res)}`)
        this.cachedRole = res.role
        sessionStorage.setItem(`parcel-management-system-role`, res.role)
        return roles.includes(res.role)
      })
    );
  }

}
