import { Component } from '@angular/core';
import { BehaviorSubject, debounceTime, distinctUntilChanged, map, Observable, switchMap } from 'rxjs';
import { LockerResponse, LockerService } from '../../locker-service';
import { ApiError } from '../../../../core/error-handling/api-catch-error';
import { ActivatedRoute } from '@angular/router';
import { ListingQueryParams } from '../../../../common/models/listing-query-params';
import { AsyncPipe } from '@angular/common';
import { formatTime } from '../../../../utils/date-time-utils';
import { FormsModule } from '@angular/forms';
import { Pagination, PaginationEmitData } from "../../../../common/components/pagination/pagination";

@Component({
  selector: 'app-listing',
  imports: [AsyncPipe, FormsModule, Pagination],
  templateUrl: './listing.html',
  styleUrl: './listing.css'
})
export class Listing {
 // pagination data 
 paginationCurrentPage = 1
 paginationPageSize = 10

  queryParams = new BehaviorSubject<ListingQueryParams>({
    page: this.paginationCurrentPage, 
    take: this.paginationPageSize
  })
  lockerList$ = this.queryParams.pipe(
    debounceTime(300), 
    distinctUntilChanged(), 
    switchMap(params => this.lockerService.getAllLockers(params)
      .pipe(
        map((res: LockerResponse | ApiError) => {
          if('lockers' in res) {
            return {
              ...res,
              lockers: res.lockers.map((locker: any) => {
                return {
                  ...locker, 
                  createdAt: formatTime(locker.createdAt),
                  updatedAt: formatTime(locker.updatedAt)
                }
              })
            }
          }
          return res
        })
      )
  )
  )


  constructor(private lockerService: LockerService, private route: ActivatedRoute) {}

  onEditClick(id:string) {

  }

  onSearch(searchKeyword: string) {
    this.queryParams.next({
      ...this.queryParams, 
      searchKeyword: searchKeyword
    })
  }

  onPaginationChanged(data: PaginationEmitData) {
    this.queryParams.next({
      ...this.queryParams, 
      page: data.currentPage, 
      take: data.pageSize
    })
  }
}
