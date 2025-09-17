import { RoleWithExp } from "../roles/role-service";

export const PERSISTENT_ROLE_KEY = 'parcel-management-system-role'

export class RoleStorage {
    setStoredRole(payload: RoleWithExp) {
        var toBeStored = JSON.stringify(payload)
        sessionStorage.setItem(PERSISTENT_ROLE_KEY, toBeStored)
        localStorage.setItem(PERSISTENT_ROLE_KEY, toBeStored)
    }

    getStoredRole(): RoleWithExp | null{
        if(sessionStorage.getItem(PERSISTENT_ROLE_KEY)) {
            return sessionStorage.getItem(PERSISTENT_ROLE_KEY) as unknown as RoleWithExp
        } 
        if(localStorage.getItem(PERSISTENT_ROLE_KEY)) {
            return localStorage.getItem(PERSISTENT_ROLE_KEY) as unknown as RoleWithExp
        }
        return null
    }

    clearStoredRole() {
        sessionStorage.setItem(PERSISTENT_ROLE_KEY, '')
        localStorage.setItem(PERSISTENT_ROLE_KEY, '')
    }
}