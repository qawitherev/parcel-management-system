import { environment } from "../../../environment/environment";

export const parcelEndpoints = {
    getAwaitingPickup: `${environment.apiBaseUrl}/parcel/awaitingPickup`,
    getRecentlyPickedUp: `${environment.apiBaseUrl}/parcel/recentlyPickedUp`
}