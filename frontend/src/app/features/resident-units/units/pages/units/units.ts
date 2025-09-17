import { Component, OnDestroy, OnInit } from '@angular/core';
import { GetAllResidentUnitsResponse, GetAllUnitsParams, ResidentUnitResponse, UnitsService } from '../../units-service';
import { BehaviorSubject, debounceTime, distinctUntilChanged, map, Observable, Subject, switchMap, take, takeUntil } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { PaginationEmitData, Pagination } from '../../../../../common/components/pagination/pagination';
import { formatTime } from '../../../../../utils/date-time-utils';
import { FormsModule } from '@angular/forms';
import { AppConsole } from '../../../../../utils/app-console';

@Component({
  selector: 'app-units',
  standalone: true,
  imports: [AsyncPipe, Pagination, FormsModule],
  templateUrl: './units.html',
  styleUrl: './units.css',
})
export class Units implements OnInit, OnDestroy {
  destroy$ = new Subject<void>();

  // pagination state
  paginationCurrentPage: number = 1;
  paginationPageSize: number = 10;

  queryParams = new BehaviorSubject<GetAllUnitsParams>({
    page: this.paginationCurrentPage,
    take: this.paginationPageSize,
  });

  unitsList$ = this.queryParams.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    switchMap((params) => this.unitsService.getAllUnits(params).pipe(
      map(res => {
        if ('residentUnits' in res) {
          return {
            ...res, 
            residentUnits: res.residentUnits.map((unit: ResidentUnitResponse) => {
              return {
                ...unit, 
                createdAt: formatTime(unit.createdAt),
                updatedAt: formatTime(unit.updatedAt!)
              }
            })
          }
        }
        return res
      })
    )),
    takeUntil(this.destroy$)
  );

  constructor(private unitsService: UnitsService) {}

  ngOnInit(): void {
    // do nothing
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onPaginationChanged(data: PaginationEmitData) {
    this.queryParams.next({
      ...this.queryParams,
      page: data.currentPage,
      take: data.pageSize,
    });
  }

  onSearch(searchKeyword: string) {
    this.queryParams.next({
      ...this.queryParams, 
      unitName: searchKeyword
    })
  }
}
