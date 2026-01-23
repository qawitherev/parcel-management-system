import { environment } from "../../../environment/environment"

export const AuthEndpoints = {
    login: `${environment.apiBaseUrl}/v1/user/login`, 
    register: `${environment.apiBaseUrl}/v1/user/register/resident`,
    registerManager: `${environment.apiBaseUrl}/v1/user/register/parcelRoomManager`, 
    refreshToken: `${environment.apiBaseUrl}/v1/token/refresh`
}