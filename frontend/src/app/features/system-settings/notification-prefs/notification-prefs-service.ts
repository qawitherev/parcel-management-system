import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { ApiError, handleApiError } from '../../../core/error-handling/api-catch-error';
import { HttpParamsBuilder } from '../../../utils/param-builder';
import { notificationPrefEndpoints } from '../../../core/endpoints/notification-prefs-endpoints';

export interface NotificationPrefCreateRequest {
  isEmailActive?: boolean;
  isWhatsAppActive?: boolean;
  isOnCheckInActive?: boolean;
  isOnClaimActive?: boolean;
  isOverdueActive?: boolean;
  quietHoursFrom?: string | null;
  quietHoursTo?: string | null;
}

export interface NotificationPrefResponse {
  id: string;
  userId: string;
  isEmailActive: boolean;
  isWhatsAppActive: boolean;
  isOnCheckInActive: boolean;
  isOnClaimActive: boolean;
  isOverdueActive: boolean;
  quietHoursFrom?: string | null;
  quietHoursTo?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationPrefsService {

  constructor(private http: HttpClient) {}
  /**
   * create notification-prefs 
   * get notifiation-prefs 
   * update notification-prefs 
   */

  createNotificationPrefs(requestPayload: NotificationPrefCreateRequest): Observable<NotificationPrefResponse | ApiError> {
    const payloadParams: HttpParams = HttpParamsBuilder(requestPayload);
    return this.http.post<NotificationPrefResponse | ApiError>(notificationPrefEndpoints.createNotificationPref, {payloadParams}).pipe(
      catchError(handleApiError)
    )
  }
}
