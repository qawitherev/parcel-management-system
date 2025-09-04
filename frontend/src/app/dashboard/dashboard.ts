import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AppConsole } from '../utils/app-console';
import { parcelEndpoints } from '../core/endpoints/parcel-endpoints';

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

@Injectable({
  providedIn: 'root'
})
export class Dashboard {

  constructor(private http: HttpClient) {

  }

  getAwaitingPickup(): Observable<any> {
    AppConsole.log(`Fetching getAwaitingPickup`)
    return this.http.get<ParcelResponseDtoList>(parcelEndpoints.getAwaitingPickup)
  }
}
