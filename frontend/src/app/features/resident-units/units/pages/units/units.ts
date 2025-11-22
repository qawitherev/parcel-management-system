import { Component, OnDestroy, OnInit } from '@angular/core';
import { GetAllResidentUnitsResponse, GetAllUnitsParams, ResidentUnit, UnitsService } from '../../units-service';
import { BehaviorSubject, combineLatest, debounce, debounceTime, distinctUntilChanged, map, Observable, Subject, switchMap, take, takeUntil, tap } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { PaginationEmitData } from '../../../../../common/components/pagination/pagination';
import { formatTime } from '../../../../../utils/date-time-utils';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ListingQueryParams } from '../../../../../common/models/listing-query-params';
import { AppConsole } from '../../../../../utils/app-console';
import { MyTable, TableColumn } from '../../../../../common/components/table/my-table/my-table';

@Component({
  selector: 'app-units',
  standalone: true,
  imports: [AsyncPipe, FormsModule, MyTable],
  templateUrl: './units.html',
  styleUrl: './units.css',
})
export class Units implements OnInit, OnDestroy {
  destroy$ = new Subject<void>();

  // pagination state
  paginationCurrentPage: number = 1;
  paginationPageSize: number = 10;

  // Type-safe table columns
  tableColumns: TableColumn<ResidentUnit>[] = [
    { key: 'unitName', label: 'Unit Name', isActionColumn: false },
    { key: 'createdBy', label: 'Created By', isActionColumn: false },
    { key: 'createdAt', label: 'Created At', isActionColumn: false },
    { key: 'updatedBy', label: 'Updated By', isActionColumn: false },
    { key: 'updatedAt', label: 'Updated At', isActionColumn: false },
    { key: 'edit', label: 'Edit', isActionColumn: true }
  ];

  keywordStream = new BehaviorSubject<string>('')
  paginationParams = new BehaviorSubject<Omit<ListingQueryParams, 'searchKeyword'>>({
    page: this.paginationCurrentPage,
    take: this.paginationPageSize
  })

  unitList$ = combineLatest([
    this.keywordStream.pipe(debounceTime(300), distinctUntilChanged()),
    this.paginationParams.pipe(distinctUntilChanged())
  ]).pipe(
    tap(value => {
      AppConsole.log(`COMBINE-LATEST: value is: ${JSON.stringify(value)}`)
    }),
    map(([searchKeyword, pagination]) => ({
      searchKeyword,
      ...pagination
    })),
    switchMap((params) => this.unitsService.getAllUnits(params).pipe(
      map(res => {
        if ('residentUnits' in res) {
          return {
            ...res,
            residentUnits: res.residentUnits.map((unit: ResidentUnit) => {
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
    ))
  )

  constructor(private unitsService: UnitsService, private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    // do nothing
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onPaginationChanged(data: PaginationEmitData) {
    this.paginationParams.next({
      page: data.currentPage,
      take: data.pageSize
    })
    this.paginationCurrentPage = data.currentPage
    this.paginationPageSize = data.pageSize
  }

  onSearch(searchKeyword: string) {
    this.keywordStream.next(searchKeyword)
  }

  onActionClicked(unit: ResidentUnit) {
    this.onEditClick(unit.id);
  }

  onEditClick(id: string) {
    this.router.navigate(['edit', id], { relativeTo: this.route })
  }

  onAddClick() {
    this.router.navigate(['edit'], { relativeTo: this.route })
  }
}
