import { Component } from '@angular/core';
import { BehaviorSubject, combineLatest, debounceTime, distinctUntilChanged, map, Observable, switchMap } from 'rxjs';
import { LockerResponse, LockerService } from '../../locker-service';
import { ApiError } from '../../../../core/error-handling/api-catch-error';
import { ActivatedRoute, Router } from '@angular/router';
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

 searchKeyword = new BehaviorSubject<string>('')
 paginationParams = new BehaviorSubject<Omit<ListingQueryParams, 'searchKeyword'>>({
  page: this.paginationCurrentPage, 
  take: this.paginationPageSize
 })

 lockerList$ = combineLatest([
  this.searchKeyword.pipe(debounceTime(300), distinctUntilChanged()), 
  this.paginationParams.pipe(distinctUntilChanged())
 ]).pipe(
  map(([searchKeyword, pagination]) => ({
    searchKeyword, 
    ...pagination
  })), 
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

  constructor(private lockerService: LockerService, private route: ActivatedRoute, 
    private router: Router
  ) {}

  onEditClick(id:string) {
    this.router.navigate(['addEdit', id], {relativeTo: this.route})
  }

  onSearch(searchKeyword: string) {
    this.searchKeyword.next(searchKeyword)
  }

  onPaginationChanged(data: PaginationEmitData) {
    this.paginationParams.next({
      page: data.currentPage, 
      take: data.pageSize
    })
  }

  onAddLockerClick() {
    this.router.navigate(['addEdit'], {relativeTo: this.route})
  }
}
