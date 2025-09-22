import { environment } from "../../../environment/environment";

export const parcelEndpoints = {
    getAwaitingPickup: `${environment.apiBaseUrl}/parcel/awaitingPickup`,
    getRecentlyPickedUp: `${environment.apiBaseUrl}/parcel/recentlyPickedUp`,
    getMyParcels: `${environment.apiBaseUrl}/parcel/myParcels`, 
    checkIn: `${environment.apiBaseUrl}/parcel/checkIn`,
    bulkCheckIn: `${environment.apiBaseUrl}/parcel/bulkCheckIn`,
    all: `${environment.apiBaseUrl}/parcel/all`
}