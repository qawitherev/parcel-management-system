import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, of } from 'rxjs';
import { environment } from '../../../../environment/environment';
import { handleApiError } from '../../../core/error-handling/api-catch-error';
import { AppConsole } from '../../../utils/app-console';

@Injectable({
  providedIn: 'root'
})
export class ClaimService {
  constructor(private http: HttpClient) {}

  claimParcel(trackingNumber: string): Observable<any> {
    return this.http.post(`${environment.apiBaseUrl}/v1/parcel/trackingNumber/${trackingNumber}/claim`, null, { observe: 'response'}).pipe(
      map((res : any) => {
        if(res.status === 204) {
          return { message: "Parcel successfully claimed"}
        } else return res
      }),
      catchError(handleApiError)
    )
  }
}
