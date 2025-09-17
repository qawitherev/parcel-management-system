import { Component, OnDestroy, OnInit } from '@angular/core';
import { GetAllResidentUnitsResponse, GetAllUnitsParams, UnitsService } from '../../units-service';
import { Observable, Subject, takeUntil } from 'rxjs';
import { ApiError } from '../../../../../core/error-handling/api-catch-error';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-units',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './units.html',
  styleUrl: './units.css'
})
export class Units implements OnInit, OnDestroy {
  unitsList$?: Observable<GetAllResidentUnitsResponse | ApiError>
  destroy$ = new Subject<void>()
  queryParams: GetAllUnitsParams = {}

  constructor(private unitsService: UnitsService) {}

  ngOnInit(): void {
    this.unitsList$ = this.unitsService.getAllUnits(this.queryParams)
      .pipe(
        takeUntil(this.destroy$)
      )
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }
}
