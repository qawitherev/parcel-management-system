import { Component, OnDestroy } from '@angular/core';
import { CheckInService } from '../../check-in-service';
import { Observable, Subject } from 'rxjs';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AppConsole } from '../../../../../utils/app-console';
import { AsyncPipe, NgIf } from '@angular/common';
import { NgClass } from '@angular/common';
import { FileUpload } from "../../../../../common/components/file-upload/file-upload";

@Component({
  selector: 'app-check-in',
  imports: [ReactiveFormsModule, NgIf, AsyncPipe, NgClass, FileUpload],
  templateUrl: './check-in.html',
  styleUrl: './check-in.css'
})
export class CheckIn implements OnDestroy {
  private destroy$ = new Subject<any>()
  formGroup: FormGroup
  checkInResponse$?: Observable<any>
  isBulkCheckInPopup: boolean = false

  constructor(private checkInService: CheckInService, private fb: FormBuilder) {
    this.formGroup = fb.group({
      trackingNumber: ['', [Validators.required, Validators.maxLength(20)]],
      residentUnit: ['', [Validators.required, Validators.maxLength(20)]]
    })
  }

  onCheckIn() {
    if (this.formGroup.valid) {
      AppConsole.log(`Check in starting...`)
      const payload = {
        trackingNumber: this.formGroup.value.trackingNumber, 
        residentUnit: this.formGroup.value.residentUnit
      }
      AppConsole.log(`payload: ${JSON.stringify(payload)}`)
      this.checkInResponse$ = this.checkInService.checkInParcel(payload)
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next(null)
    this.destroy$.complete()
  }

  onBulkCheckInPopup() {
    this.isBulkCheckInPopup = !this.isBulkCheckInPopup
  }
}
