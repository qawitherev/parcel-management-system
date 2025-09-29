import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { parcelEndpoints } from '../../../core/endpoints/parcel-endpoints';
import { handleApiError } from '../../../core/error-handling/api-catch-error';
import { AppConsole } from '../../../utils/app-console';

export interface CheckInPayload {
  trackingNumber: string, 
  residentUnit: string, 
  locker: string,
  weight?: number, 
  dimension?: string
}

export interface BulkCheckInError {
  status: string, 
  parcelCheckedIn: number, 
  message: string, 
  errors: BulkCheckInErrorChild[]
}

interface BulkCheckInErrorChild {
  row: number, 
  errorDetail: string
}

@Injectable({
  providedIn: 'root'
})
export class CheckInService {
  constructor(private http: HttpClient) {}

  checkInParcel(payload: CheckInPayload): Observable<any> {
    return this.http.post(parcelEndpoints.checkIn, payload).pipe(
      catchError(err => {
        return of({ error: true, message: err.error.message})
      })
    )
  }

  bulkCheckInParcel(payload: CheckInPayload[]): Observable<any | BulkCheckInError> {
    return this.http.post(parcelEndpoints.bulkCheckIn, payload).pipe(
      catchError(error => {
        AppConsole.log(`DEV: ${JSON.stringify(error)}`)
        return of({
        error: true,
        status: error.error.status, 
        parcelCheckedIn: 0, 
        message: error.error.message, 
        errors: error.error.error.map((e: any) => ({
          row: e.row, 
          errorDetail: e.errorDetail
        }))
      })})
    )
  }
}
