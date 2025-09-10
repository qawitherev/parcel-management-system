import { HttpClient } from "@angular/common/http"
import { catchError, map, Observable, of, tap } from "rxjs"
import { RoleStorage } from "../storage/role-storage"
import { environment } from "../../../environment/environment"
import { handleApiError } from "../error-handling/api-catch-error"
import { AppConsole } from "../../utils/app-console"

export interface Role {
    role: string, 
    expiredAt: number
}

export class RoleService {
    private _role: Role
    private _roleStorage: RoleStorage
    constructor(private http: HttpClient) {
        this._role = { role: "", expiredAt: 0}
        this._roleStorage = new RoleStorage()
    }

    setRole(newRole: string, ttlMinutes = 60) {
        this.clearRole()
        const expiryTime = Date.now() + (ttlMinutes * 60 * 1000)
        this._role = { role: newRole, expiredAt: expiryTime}
        this._roleStorage.setStoredRole(this._role)
    }

    getRole(): Observable<Role | null> {
        var storedRole = this._roleStorage.getStoredRole()
        if (storedRole && this.isExpired(storedRole.expiredAt)) {
            this._role = storedRole
            return of(storedRole)
        } 
        this.clearRole()
        return this.getRoleByApi().pipe(
            map((res: any) => {
                if(res.error) return null
                this.setRole(res.role)
                return this._role
            })
        )
    }

    clearRole() {
        this._roleStorage.clearStoredRole()
    }

    private getRoleByApi(): Observable<any> {
        return this.http.get(`${environment.apiBaseUrl}/user/basic`).pipe(
            map((res: any) => {
                return { role: res.role }
            }),
            catchError(handleApiError)
        )
    }
    
    private isExpired(expiredAt: number): boolean {
        return Date.now() > expiredAt
    }
}

export class CachedRoleService {
    private _cachedRole: string | null = null

    getCachedRole() : string | null {
        return this._cachedRole
    }

    setCachedRole(newRole: string) {
        this._cachedRole = newRole
    }

    clearRole() {
        this._cachedRole = null
    }
}
