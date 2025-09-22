import { HttpClient } from "@angular/common/http"
import { catchError, map, Observable, of, tap } from "rxjs"
import { RoleStorage } from "../storage/role-storage"
import { environment } from "../../../environment/environment"
import { handleApiError } from "../error-handling/api-catch-error"
import { AppConsole } from "../../utils/app-console"
import { Injectable } from "@angular/core"

export interface RoleWithExp {
    role: string, 
    expiredAt: number
}

@Injectable({
  providedIn: 'root',
})
export class RoleService {
    
    private _role: RoleWithExp
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

    getRole(): Observable<RoleWithExp | null> {
        AppConsole.log(`BASIC: entered get role`)
        var storedRole = this._roleStorage.getStoredRole()
        AppConsole.log(`BASIC: stored role is ${storedRole}`)
        if (storedRole && !this.isExpired(storedRole.expiredAt)) {
            
            this._role = storedRole
            return of(storedRole)
        } 
        this.clearRole()
        return this.getRoleByApi().pipe(
            map((res: any) => {
                if(res.error) {
                    AppConsole.log(`ROLE: error getRoleByApi: ${res.error}`)
                    return null
                }
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
        AppConsole.log(`BASIC: isexpired: ${Date.now() > expiredAt}`)
        return Date.now() > expiredAt
    }
}
