import { Component } from '@angular/core';
import { BehaviorSubject, combineLatest, debounceTime, distinctUntilChanged, map, Observable, switchMap } from 'rxjs';
import { LockerResponse, LockerService } from '../../locker-service';
import { ApiError } from '../../../../core/error-handling/api-catch-error';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingQueryParams } from '../../../../common/models/listing-query-params';
import { AsyncPipe } from '@angular/common';
import { formatTime } from '../../../../utils/date-time-utils';
import { FormsModule } from '@angular/forms';
import { PaginationEmitData } from "../../../../common/components/pagination/pagination";
import { MySearchbar } from "../../../../common/components/searchbar/my-searchbar/my-searchbar";
import { MyButton } from "../../../../common/components/buttons/my-button/my-button";
import { MyTable, TableColumn } from "../../../../common/components/table/my-table/my-table";

@Component({
  selector: 'app-listing',
  imports: [AsyncPipe, FormsModule, MySearchbar, MyButton, MyTable],
  templateUrl: './listing.html',
  styleUrl: './listing.css'
})
export class Listing {

  tableColumns: TableColumn<any>[] = [
    { key: 'lockerName', label: 'Locker', isActionColumn: false },
    { key: 'createdBy', label: 'Created By', isActionColumn: false },
    { key: 'createdAt', label: 'Created At', isActionColumn: false },
    { key: 'updatedBy', label: 'Updated By', isActionColumn: false },
    { key: 'updatedAt', label: 'Updated At', isActionColumn: false },
    { key: 'edit', label: 'Edit', isActionColumn: true },
  ]

  // pagination data 
  paginationCurrentPage = 1
  paginationPageSize = 10

  searchKeyword = new BehaviorSubject<string>('')
  paginationParams = new BehaviorSubject<Omit<ListingQueryParams, 'searchKeyword'>>({
    page: this.paginationCurrentPage,
    take: this.paginationPageSize
  })

  lockerList$ = combineLatest([
    this.searchKeyword,
    this.paginationParams.pipe(distinctUntilChanged())
  ]).pipe(
    map(([searchKeyword, pagination]) => ({
      searchKeyword,
      ...pagination
    })),
    switchMap(params => this.lockerService.getAllLockers(params)
      .pipe(
        map((res: LockerResponse | ApiError) => {
          if ('lockers' in res) {
            return {
              ...res,
              lockers: res.lockers.map((locker: any, index: number) => {
                return {
                  ...locker,
                  index: ((params.page ?? 1) - 1) * (params.take ?? 10) + index + 1,
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
  ) { }

  onEditClick(id: string) {
    this.router.navigate(['addEdit', id], { relativeTo: this.route })
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
    this.router.navigate(['addEdit'], { relativeTo: this.route })
  }

  onActionClicked(locker: any) {
    this.onEditClick(locker.id);
  }
}
