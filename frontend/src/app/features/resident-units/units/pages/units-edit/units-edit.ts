import { Component, input, OnDestroy, OnInit } from '@angular/core';
import { ResidentUnit, UnitsService } from '../../units-service';
import { Observable, Subject, switchMap, takeUntil, tap, map, EMPTY } from 'rxjs';
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
export class UnitsEdit implements OnInit {
  formGroup: FormGroup;
  theUnit$?: Observable<ResidentUnit | ApiError>;
  updateResponse$?: Observable<any>
  unitId?: string;

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
    this.theUnit$ = this.route.paramMap.pipe(
      map(params => params.get('id')), 
      switchMap(id => {
        if (id) {
          return this.unitService.getUnit(id)
        } else {
          return EMPTY
        }
      }), 
      tap(res => {
        if (!('error' in res)) {
          this.formGroup.get('unitName')?.patchValue(res.unitName)
          this.unitId = res.id
        }
      })
    )
  }

  onCreateOrUpdate() {
    if (this.formGroup.valid) {
      if (this.unitId) {
        this.updateResponse$ = this.unitService.updateUnit(this.unitId, this.formGroup.get('unitName')?.value).pipe(
          tap(res => {
            if (!(res && 'error' in res)) {
              this.onBack()
            }
          })
        )
      } else {
        this.updateResponse$ = this.unitService.createUnit({
          unitName: this.formGroup.get('unitName')?.value
        }).pipe(
          tap(res => {
            if (!(res && 'error' in res)) {
              this.onBack()
            }
          })
        )
      }
    }
  }

  onBack(): void {
    this.router.navigateByUrl('residentUnit/units')
  }
}
