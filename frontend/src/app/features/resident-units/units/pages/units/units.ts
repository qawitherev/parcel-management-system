import { Component, OnDestroy, OnInit } from '@angular/core';
import { GetAllResidentUnitsResponse, GetAllUnitsParams, UnitsService } from '../../units-service';
import { BehaviorSubject, Observable, Subject, switchMap, take, takeUntil } from 'rxjs';
import { ApiError } from '../../../../../core/error-handling/api-catch-error';
import { AsyncPipe } from '@angular/common';
import { PaginationEmitData, Pagination } from '../../../../../common/components/pagination/pagination';

@Component({
  selector: 'app-units',
  standalone: true,
  imports: [AsyncPipe, Pagination],
  templateUrl: './units.html',
  styleUrl: './units.css',
})
export class Units implements OnInit, OnDestroy {
  destroy$ = new Subject<void>();

  // pagination state
  paginationCurrentPage: number = 1;
  paginationPageSize: number = 10;

  private queryParams = new BehaviorSubject<GetAllUnitsParams>({
    page: this.paginationCurrentPage,
    take: this.paginationPageSize,
  });

  unitsList$ = this.queryParams.pipe(
    switchMap((params) => this.unitsService.getAllUnits(params)),
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
}
