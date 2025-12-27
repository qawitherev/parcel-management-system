import { environment } from "../../../environment/environment";

export const parcelEndpoints = {
    getAwaitingPickup: `${environment.apiBaseUrl}/v1/parcel/awaitingPickup`,
    claimParcel: `${environment.apiBaseUrl}/v1/parcel/trackingNumber`,
    bulkClaim: `${environment.apiBaseUrl}/v1/parcel/bulkClaim`,
    getRecentlyPickedUp: `${environment.apiBaseUrl}/v1/parcel/recentlyPickedUp`,
    getMyParcels: `${environment.apiBaseUrl}/v1/parcel/myParcels`, 
    checkIn: `${environment.apiBaseUrl}/v2/parcel/checkIn`,
    bulkCheckIn: `${environment.apiBaseUrl}/v1/parcel/bulkCheckIn`,
    all: `${environment.apiBaseUrl}/v1/parcel/all`
}