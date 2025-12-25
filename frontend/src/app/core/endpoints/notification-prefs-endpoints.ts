import { environment } from "../../../environment/environment";

export const notificationPrefEndpoints = {
    createNotificationPref: `${environment.apiBaseUrl}/v1/notificationPref`,
    getNotificationPref: `${environment.apiBaseUrl}/v1/notificationPref`, 
    getNotificationPrefByUser: `${environment.apiBaseUrl}/v1/notificationPref/me`, 
    updateNotificationPref: `${environment.apiBaseUrl}/v1/notificationPref`
}