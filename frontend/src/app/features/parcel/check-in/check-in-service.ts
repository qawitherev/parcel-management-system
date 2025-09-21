import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { parcelEndpoints } from '../../../core/endpoints/parcel-endpoints';

export interface CheckInPayload {
  trackingNumber: string, 
  residentUnit: string, 
  weight?: number, 
  dimension?: string
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
}
