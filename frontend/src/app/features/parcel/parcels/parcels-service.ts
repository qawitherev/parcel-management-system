import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { parcelEndpoints } from '../../../core/endpoints/parcel-endpoints';
import { handleApiError } from '../../../core/error-handling/api-catch-error';

interface GetAllParcelsRequest {
  trackingNumber?: string, 
    status?: string, 
    customEvent?: string, 
    page?: number, 
    take?: number
}

export interface ParcelResponse {
  id: string, 
  trackingNumber: string,
  weight?: number, 
  dimensions?: string, 
  residentUnit: string, 
  status: string
}

export interface ParcelResponseList {
  parcels: ParcelResponse[], 
  count: number
}

@Injectable({
  providedIn: 'root'
})
export class ParcelsService {

  constructor(private http: HttpClient) {}

  getAllParcels(
    trackingNumber?: string, 
    status?: string, 
    customEvent?: string, 
    page?: number, 
    take?: number
  ): Observable<any> {
    const payload: GetAllParcelsRequest = {
      trackingNumber,
      status,
      customEvent,
      page,
      take
    };
    return this.http.post(parcelEndpoints.all, payload).pipe(
      catchError(handleApiError)
    )
  }
}
