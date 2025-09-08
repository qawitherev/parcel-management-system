import { Component, OnInit } from '@angular/core';
import { ResultItem } from "../../components/result-item/result-item";
import { NgFor } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AppConsole } from '../../../../../utils/app-console';
import { FormControl, ɵInternalFormsSharedModule, ReactiveFormsModule } from '@angular/forms';
import { Tracking } from '../tracking/tracking';
import { TrackingModule } from '../../tracking-module';
import { TrackingService } from '../../../tracking';
import { catchError, Observable, of } from 'rxjs';
import { NgIf, AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-search-result',
  imports: [ResultItem, NgFor, ɵInternalFormsSharedModule, ReactiveFormsModule, NgIf, AsyncPipe],
  templateUrl: './search-result.html',
  styleUrl: './search-result.css'
})
export class SearchResult implements OnInit {
  parcelsTrackingHistory$?: Observable<any>
  parcels = [
    { item: '1'}, 
    { item: '2'}, 
    { item: '3'},
    { item: '1'}, 
    { item: '2'}, 
    { item: '3'},
    { item: '1'}, 
    { item: '2'}, 
    { item: '3'},
  ]
  constructor(
    private trackingService: TrackingService,
    private route: ActivatedRoute
  ) {}

  searchKeyword = new FormControl()
  ngOnInit(): void {
    this.route.queryParams.subscribe(params => 
    {
      const keyword = params['keyword']
      this.searchKeyword.setValue(keyword)
      this.parcelsTrackingHistory$ =  this.trackingService.getUserParcelHistory(keyword).pipe(
        catchError(err => {
          return of( { error: true, message: err.error.message})
        })
      )
    }
    )
  }
}
