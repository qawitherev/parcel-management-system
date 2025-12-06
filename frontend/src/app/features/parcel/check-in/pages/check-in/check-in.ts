import { Component, OnDestroy } from '@angular/core';
import {  CheckInPayload, CheckInService } from '../../check-in-service';
import { Observable, Subject, tap } from 'rxjs';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AppConsole } from '../../../../../utils/app-console';
import { AsyncPipe, NgClass } from '@angular/common';
import { FileUpload } from "../../../../../common/components/file-upload/file-upload";
import { mapperCheckInPayload } from '../../../../../core/bulk-action/excel-to-json';
import { FormFieldConfig, MyForm } from '../../../../../common/components/form/my-form/my-form';
import { MyButton } from "../../../../../common/components/buttons/my-button/my-button";

@Component({
  selector: 'app-check-in',
  imports: [ReactiveFormsModule, MyForm, AsyncPipe, MyButton, FileUpload],
  templateUrl: './check-in.html',
  styleUrl: './check-in.css'
})
export class CheckIn implements OnDestroy {
  private destroy$ = new Subject<any>()
  formGroup: FormGroup
  checkInResponse$?: Observable<any>
  bulkCheckInResponse$?: Observable<any>
  isBulkCheckInPopup: boolean = false
  payloadMapper: (data: any) => CheckInPayload = mapperCheckInPayload
  isCheckingIn: boolean = false;

  formFieldsConfig: FormFieldConfig[] = [
    {
      controlName: "trackingNumber", 
      label: "Tracking Number", 
      placeholder: "Enter tracking number", 
      type: "text", 
      errorMessageV2: {
        required: 'Tracking number is required', 
        maxlength: 'Tracking number cannot be more than 20 characters'
      }
    }, 
    {
      controlName: "residentUnit", 
      label: "Resident Unit", 
      placeholder: "Enter resident unit", 
      type: "text"
    }, 
    {
      controlName: "locker", 
      label: "Locker", 
      placeholder: "Enter locker", 
      type: "text"
    }, 
    {
      controlName: "weight", 
      label: "Weight", 
      placeholder: "Enter weight", 
      type: "text"
    }, 
    {
      controlName: "dimension", 
      label: "Dimension", 
      placeholder: "Enter dimension", 
      type: "text"
    }
  ]

  constructor(private checkInService: CheckInService, private fb: FormBuilder) {
    this.formGroup = fb.group({
      trackingNumber: ['', [Validators.required, Validators.maxLength(20)]],
      residentUnit: ['', [Validators.required, Validators.maxLength(20)]], 
      locker: ['', [Validators.required, Validators.maxLength(20)]], 
      weight: ['', [Validators.required]], 
      dimension: ['']
    })
  }

  onCheckIn(formValue: CheckInPayload) {
    this.isCheckingIn = true;
    this.checkInResponse$ = this.checkInService.checkInParcel(formValue);
    this.isCheckingIn = false;
  }

  ngOnDestroy(): void {
    this.destroy$.next(null)
    this.destroy$.complete()
  }

  onBulkCheckInPopup() {
    this.isBulkCheckInPopup = !this.isBulkCheckInPopup
  }

  onUploadFinished(data: CheckInPayload[]) {
    this.bulkCheckInResponse$ = this.checkInService.bulkCheckInParcel(data).pipe(
      tap(res => AppConsole.log(`RES: res is ${JSON.stringify(res)}`)
      )
    )
  }
}