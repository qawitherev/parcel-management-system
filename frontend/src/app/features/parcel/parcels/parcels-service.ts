import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { parcelEndpoints } from '../../../core/endpoints/parcel-endpoints';
import { handleApiError } from '../../../core/error-handling/api-catch-error';
import { HttpParamsBuilder } from '../../../utils/param-builder';

interface GetAllParcelsRequest {
  searchKeyword?: string;
  status?: string;
  customEvent?: string;
  page?: number;
  take?: number;
}

export interface ParcelResponse {
  id: string;
  trackingNumber: string;
  locker?: string;
  weight?: number;
  dimensions?: string;
  residentUnit: string;
  status: string;
}

export interface ParcelResponseList {
  parcels: ParcelResponse[];
  count: number;
}

@Injectable({
  providedIn: 'root',
})
export class ParcelsService {
  constructor(private http: HttpClient) {}

  getAllParcels(
    inParams: GetAllParcelsRequest
  ): Observable<any> {
    const params: HttpParams = HttpParamsBuilder(inParams)
    return this.http.get(parcelEndpoints.all, {params}).pipe(catchError(handleApiError));
  }
}
