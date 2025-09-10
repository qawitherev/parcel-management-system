import { Role } from "../roles/role-service";

const PERSISTENT_ROLE_KEY = 'parcel-management-system-role'

export class RoleStorage {
    setStoredRole(payload: Role) {
        var toBeStored = JSON.stringify(payload)
        sessionStorage.setItem(PERSISTENT_ROLE_KEY, toBeStored)
        localStorage.setItem(PERSISTENT_ROLE_KEY, toBeStored)
    }

    getStoredRole(): Role | null{
        if(sessionStorage.getItem(PERSISTENT_ROLE_KEY)) {
            return sessionStorage.getItem(PERSISTENT_ROLE_KEY) as unknown as Role
        } 
        if(localStorage.getItem(PERSISTENT_ROLE_KEY)) {
            return localStorage.getItem(PERSISTENT_ROLE_KEY) as unknown as Role
        }
        return null
    }

    clearStoredRole() {
        sessionStorage.removeItem(PERSISTENT_ROLE_KEY)
        localStorage.removeItem(PERSISTENT_ROLE_KEY)
    }
}