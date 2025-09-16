import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { residentUnitsEndpoints } from '../../../core/endpoints/resident-units-endpoints';
import { HttpParamsBuilder } from '../../../utils/param-builder';

export interface GetAllUnitsParams {
  unitName?: string, 
  column?: string, 
  isAsc?: boolean, 
  page?: number, 
  take?: number
}

@Injectable({
  providedIn: 'root'
})
export class UnitsService {

  constructor(
    private http: HttpClient
  ) {}

  getAllUnits(queryParams: GetAllUnitsParams) {
    const params: HttpParams = HttpParamsBuilder(queryParams)
    return this.http.get(residentUnitsEndpoints.getAllUnits, {params});
  }
}
