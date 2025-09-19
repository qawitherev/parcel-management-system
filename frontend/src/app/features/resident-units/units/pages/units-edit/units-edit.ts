import { Component, input, OnDestroy, OnInit } from '@angular/core';
import { ResidentUnit, UnitsService } from '../../units-service';
import { Observable, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { ApiError } from '../../../../../core/error-handling/api-catch-error';
import { ActivatedRoute, Router } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  ɵInternalFormsSharedModule,
} from '@angular/forms';
import { AppConsole } from '../../../../../utils/app-console';
import { AsyncPipe, NgClass } from '@angular/common';

@Component({
  selector: 'app-units-edit',
  standalone: true,
  imports: [ReactiveFormsModule, ɵInternalFormsSharedModule, AsyncPipe, NgClass],
  templateUrl: './units-edit.html',
  styleUrl: './units-edit.css',
})
export class UnitsEdit implements OnInit, OnDestroy {
  formGroup: FormGroup;
  theUnit$?: Observable<ResidentUnit | ApiError>;
  updateResponse$?: Observable<ApiError | void>
  id?: string;
  destroy$ = new Subject<void>()

  constructor(
    private unitService: UnitsService,
    private route: ActivatedRoute,
    private fb: FormBuilder, 
    private router: Router
  ) {
    this.formGroup = fb.group({
      unitName: ['', [Validators.required]],
    });
  }

  ngOnInit(): void {
    this.theUnit$ = this.route.params.pipe(
      switchMap((param) => this.unitService.getUnit(param['id'])),
      tap((unit) => {
        if ('unitName' in unit) {
          this.formGroup?.patchValue({
            unitName: unit.unitName,
          });
          this.id = unit.id;
        }
      })
    );
    this.theUnit$.pipe(
      takeUntil(this.destroy$)
    ).subscribe()
    AppConsole.log(`unit name is ${this.formGroup.get('unitName')?.value}`);
  }

  onUpdate(): void {
    if (this.formGroup?.valid && this.id != null) {
      AppConsole.log(`ONUPDATE`)
      this.updateResponse$ = this.unitService.updateUnit(this.id, this.formGroup.get('unitName')?.value).pipe(
        tap(res => {
          if (!(res && 'error' in res)) {
            this.router.navigateByUrl('residentUnit/units')
          }
        })
      )
    }
  }

  onBack(): void {
    this.router.navigateByUrl('residentUnit/units')
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }
}
