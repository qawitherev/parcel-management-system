import { AppConsole } from "../../utils/app-console";
import { RoleWithExp } from "../roles/role-service";

export const PERSISTENT_ROLE_KEY = 'parcel-management-system-role'

export class RoleStorage {
    setStoredRole(payload: RoleWithExp) {
        var toBeStored = JSON.stringify(payload)
        sessionStorage.setItem(PERSISTENT_ROLE_KEY, toBeStored)
        localStorage.setItem(PERSISTENT_ROLE_KEY, toBeStored)
    }

    getStoredRole(): RoleWithExp | null{
        var storedSession = sessionStorage.getItem(PERSISTENT_ROLE_KEY)
        var storedLocal = localStorage.getItem(PERSISTENT_ROLE_KEY)
        let roleWithExp: RoleWithExp
        if(storedSession) {
            try {
                roleWithExp = JSON.parse(storedSession) as RoleWithExp
                return roleWithExp
            } catch (err) {
                AppConsole.error(`Failed to try parse into RoleWithExp: ${err}`)
                return null
            }
        } 

        if(storedLocal) {
            try {
                roleWithExp = JSON.parse(storedLocal) as RoleWithExp
                return roleWithExp
            } catch (err) {
                AppConsole.error(`Failed to try parse into RoleWithExp: ${err}`)
                return null
            }
        }
        return null
    }

    clearStoredRole() {
        sessionStorage.setItem(PERSISTENT_ROLE_KEY, '')
        localStorage.setItem(PERSISTENT_ROLE_KEY, '')
    }
}