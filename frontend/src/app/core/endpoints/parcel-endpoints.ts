import { environment } from "../../../environment/environment";

export const parcelEndpoints = {
    getAwaitingPickup: `${environment.apiBaseUrl}/parcel/awaitingPickup`,
    getRecentlyPickedUp: `${environment.apiBaseUrl}/parcel/recentlyPickedUp`,
    getMyParcels: `${environment.apiBaseUrl}/parcel/myParcels`, 
    checkIn: `${environment.apiBaseUrl}/parcel/checkIn`
    
}