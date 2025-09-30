import { environment } from "../../../environment/environment";

export const residentUnitsEndpoints = {
    createUpdateUnit: `${environment.apiBaseUrl}/v1/residentUnit`,
    getAllUnits: `${environment.apiBaseUrl}/v1/residentUnit/all`
}