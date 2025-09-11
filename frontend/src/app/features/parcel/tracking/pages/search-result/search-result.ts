import { Component, OnInit } from '@angular/core';
import { ResultItem } from "../../components/result-item/result-item";
import { CommonModule, NgFor } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AppConsole } from '../../../../../utils/app-console';
import { FormControl, ɵInternalFormsSharedModule, ReactiveFormsModule } from '@angular/forms';
import { Tracking } from '../tracking/tracking';
import { TrackingModule } from '../../tracking-module';
import { TrackingService } from '../../../tracking';
import { catchError, Observable, of } from 'rxjs';
import { NgIf, AsyncPipe } from '@angular/common';
import { handleApiError } from '../../../../../core/error-handling/api-catch-error';

export interface ParcelHistory {
  trackingNumber: string, 
  entryDate: Date, 
  currentStatus: string, 
  history: ParcelHistoryItem[]
}

export interface ParcelHistoryItem {
  eventTime: Date, 
  event: string, 
  performedByUser: string
}

@Component({
  selector: 'app-search-result',
  imports: [ResultItem, NgFor, ɵInternalFormsSharedModule, ReactiveFormsModule, NgIf, AsyncPipe, CommonModule],
  templateUrl: './search-result.html',
  styleUrl: './search-result.css'
})
export class SearchResult implements OnInit {
  parcelsTrackingHistory$?: Observable<any>

  constructor(
    private trackingService: TrackingService,
    private route: ActivatedRoute
  ) {}

  searchKeyword = new FormControl()

  onSearch() {
    AppConsole.log(`onSearch() not implemented yet`)
    this.parcelsTrackingHistory$ = this.trackingService.getUserParcelHistory(this.searchKeyword.value).pipe(
      catchError(handleApiError)
    )
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => 
    {
      const keyword = params['keyword']
      this.searchKeyword.setValue(keyword)
      this.parcelsTrackingHistory$ =  this.trackingService.getUserParcelHistory(keyword).pipe(
        catchError(handleApiError)
      )
    }
    )
  }
}
