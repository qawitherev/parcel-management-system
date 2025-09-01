import { environment } from "../../../environment/environment"

export const AuthEndpoints = {
    login: `${environment.apiBaseUrl}/user/login`, 
    register: `${environment.apiBaseUrl}user/register/resident`
}