import { environment } from "../../../environment/environment";

export const lockerEndpoints = {
    createUpdateLocker: `${environment.apiBaseUrl}/v1/locker`,
    getAllLockers: `${environment.apiBaseUrl}/v1/locker/all`
}