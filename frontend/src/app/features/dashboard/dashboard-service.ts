import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { AppConsole } from '../../utils/app-console';
import { parcelEndpoints } from '../../core/endpoints/parcel-endpoints';
import { UserEndpoints } from '../../core/endpoints/user-endpoints';
import { ApiError, handleApiError } from '../../core/error-handling/api-catch-error';

interface ParcelResponseDto {
  Id: string, 
  TrackingNumber: string, 
  Weight: number, 
  Dimensions: string
}

interface ParcelResponseDtoList {
  Parcels: ParcelResponseDto[], 
  Count: number
}

export interface UserResponse {
  Id: string, 
  Username: string, 
  Role: string
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  constructor(private http: HttpClient) {

  }

  getAwaitingPickup(): Observable<any> {
    AppConsole.log(`Fetching getAwaitingPickup`)
    return this.http.get<ParcelResponseDtoList>(parcelEndpoints.getAwaitingPickup)
  }

  getRecentlyPickedUp(): Observable<any> {
    AppConsole.log(`Fetching recentlyPickedUp`)
    return this.http.get(parcelEndpoints.getRecentlyPickedUp)
  }

  getUserAwaitingPickup(): Observable<any> {
    return this.http.get(`${parcelEndpoints.getMyParcels}/awaitingPickup`)
  }

  getUserDetails(): Observable<UserResponse | ApiError> {
    return this.http.get<UserResponse | ApiError>(`${UserEndpoints.me}`).pipe(
      catchError(handleApiError)
    )
  }
}
