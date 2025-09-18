import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { residentUnitsEndpoints } from '../../../core/endpoints/resident-units-endpoints';
import { HttpParamsBuilder } from '../../../utils/param-builder';
import { catchError, Observable } from 'rxjs';
import { ApiError, handleApiError } from '../../../core/error-handling/api-catch-error';
import { environment } from '../../../../environment/environment';

export interface GetAllUnitsParams {
  unitName?: string, 
  column?: string, 
  isAsc?: boolean, 
  page?: number, 
  take?: number
}

export interface GetAllResidentUnitsResponse {
  residentUnits: ResidentUnit[], 
  count: number
}

export interface ResidentUnit {
  id: string, 
  unitName: string, 
  createdAt: string, 
  createdBy: string, 
  updatedAt?: string, 
  updatedBy?: string
}

@Injectable({
  providedIn: 'root'
})
export class UnitsService {

  constructor(
    private http: HttpClient
  ) {}

  getAllUnits(queryParams: GetAllUnitsParams): Observable<GetAllResidentUnitsResponse | ApiError> {
    const params: HttpParams = HttpParamsBuilder(queryParams)
    return this.http.get<GetAllResidentUnitsResponse | ApiError>(residentUnitsEndpoints.getAllUnits, {params})
      .pipe(
        catchError(handleApiError)
      )
  }

  getUnit(id: string): Observable<ResidentUnit | ApiError> {
    return this.http.get<ResidentUnit | ApiError>(`${environment.apiBaseUrl}/residentUnit/${id}`)
      .pipe(
        catchError(handleApiError)
      )
  }

  updateUnit(unit: ResidentUnit): Observable<any> {
    return this.http.patch(`${environment.apiBaseUrl}/residentUnit/${unit.id}`, unit.unitName)
  }
}
