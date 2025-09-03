import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AppConsole } from '../utils/app-console';
import { parcelEndpoints } from '../core/endpoints/parcel-endpoints';

interface AwaitingPickupItem {
  Id: string, 
  TrackingNumber: string, 
  Weight: number, 
  Dimension: string
}

@Injectable({
  providedIn: 'root'
})
export class Dashboard {

  constructor(private http: HttpClient) {

  }

  getAwaitingPickup(): Observable<any> {
    AppConsole.log(`Fetching getAwaitingPickup`)
    return this.http.get<AwaitingPickupItem[]>(parcelEndpoints.getAwaitingPickup)
  }
}
