import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { ApiError, handleApiError } from '../../core/error-handling/api-catch-error';
import { lockerEndpoints } from '../../core/endpoints/locker-endpoints';
import { ListingQueryParams } from '../../common/models/listing-query-params';
import { HttpParamsBuilder } from '../../utils/param-builder';
import { environment } from '../../../environment/environment';

export interface LockerResponseChild {
  id: string, 
  lockerName: string, 
  createdBy: string, 
  createdAt: string, 
  updatedBy?: string, 
  updatedAt?: string
}

export interface LockerResponse {
  count: number, 
  lockers: LockerResponseChild[]
}

export interface CreateUpdateLockerRequest {
  lockerName: string
}

@Injectable({
  providedIn: 'root'
})
export class LockerService {

  constructor(private http: HttpClient) {}

  getAllLockers(queryParams: ListingQueryParams): Observable<LockerResponse | ApiError> {
    const params: HttpParams = HttpParamsBuilder(queryParams)
    return this.http.get<LockerResponse | ApiError>(lockerEndpoints.getAllLockers, {params}).pipe(
      catchError(handleApiError)
    )
  }

  getLocker(id: string): Observable<LockerResponseChild | ApiError> {
    return this.http.get<LockerResponseChild | ApiError>(`${environment.apiBaseUrl}/v1/locker/${id}`)
      .pipe(
        catchError(handleApiError)
      )
  }

  createLocker(payload: CreateUpdateLockerRequest): Observable<LockerResponseChild | ApiError> {
    return this.http.post<LockerResponseChild | ApiError>(lockerEndpoints.createUpdateLocker, payload)
    .pipe(
      catchError(handleApiError)
    )
  }

  updateLocker(payload: CreateUpdateLockerRequest, id: string): Observable<null | ApiError> {
    return this.http.patch<null | ApiError>(lockerEndpoints.createUpdateLocker, {payload})
      .pipe(
        catchError(handleApiError)
      )
  }
}
