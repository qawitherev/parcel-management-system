import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment/environment';
import { AppConsole } from '../../utils/app-console';

@Injectable({
  providedIn: 'root'
})
export class TrackingService {
  constructor(private http: HttpClient) {}

  getUserParcelHistory(trackingNumber: string): Observable<any> {
    AppConsole.log(`API call: getParcelHistory`)
    return this.http.get(`${environment.apiBaseUrl}/v1/parcel/trackingNumber/${trackingNumber}/history`)
  }
}
