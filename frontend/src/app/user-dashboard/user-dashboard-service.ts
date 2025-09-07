import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { parcelEndpoints } from '../core/endpoints/parcel-endpoints';

@Injectable({
  providedIn: 'root'
})
export class UserDashboardService {
  
  constructor(private http: HttpClient) {}

  getUserAwaitingPickup(): Observable<any> {
    return this.http.get(`${parcelEndpoints.getMyParcels}/awaitingPickup`)
  }
}
