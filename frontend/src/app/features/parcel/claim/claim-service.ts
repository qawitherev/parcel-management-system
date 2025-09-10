import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of } from 'rxjs';
import { environment } from '../../../../environment/environment';
import { handleApiError } from '../../../core/error-handling/api-catch-error';

@Injectable({
  providedIn: 'root'
})
export class ClaimService {
  constructor(private http: HttpClient) {}

  claimParcel(trackingNumber: string): Observable<any> {
    return this.http.post(`${environment.apiBaseUrl}/${trackingNumber}/claim`, null).pipe(
      catchError(handleApiError)
    )
  }
}
