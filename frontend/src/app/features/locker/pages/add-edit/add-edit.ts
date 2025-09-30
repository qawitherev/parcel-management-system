import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EMPTY, map, Observable, of, switchMap, tap } from 'rxjs';
import { LockerResponseChild, LockerService } from '../../locker-service';
import { ApiError } from '../../../../core/error-handling/api-catch-error';
import { AsyncPipe, NgClass } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-add-edit',
  imports: [AsyncPipe, ReactiveFormsModule, NgClass],
  templateUrl: './add-edit.html',
  styleUrl: './add-edit.css'
})
export class AddEdit implements OnInit {
  formGroup: FormGroup
  lockerId?: string
  lockerDetails$?: Observable<LockerResponseChild | ApiError>
  createUpdateRes$?: Observable<any>
  constructor(private route: ActivatedRoute, 
    private lockerService: LockerService, 
    private fb: FormBuilder,
    private router: Router
  ) {
    this.formGroup = fb.group({
      lockerName: ['', [Validators.required, Validators.maxLength(20)]]
    })
  }

  ngOnInit(): void {
    this.lockerDetails$ = this.route.paramMap.pipe(
      map(params => params.get('id')),
      switchMap(id => {
        if(id) {
          return this.lockerService.getLocker(id)
        } else {
          return EMPTY
        }
      }),
      tap(res => {
        if ('lockerName' in res) {
          this.lockerId = res.id
          this.formGroup.get('lockerName')?.patchValue(res.lockerName)
        }
      })
    )
  }

  onCreateOrUpdate() {
    if(this.formGroup.valid) {
      if (this.lockerId) {
        this.createUpdateRes$ = this.lockerService.updateLocker({
          lockerName: this.formGroup.get('lockerName')?.value
        }, this.lockerId).pipe(
          tap(res => {
            if(!(res && 'error' in res)) {
              this.onBack()
            }
          })
        )
    } else {
      this.createUpdateRes$ = this.lockerService.createLocker({
        lockerName: this.formGroup.get('lockerName')?.value
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

  onBack() {
    this.router.navigateByUrl('/locker')
  }
}
