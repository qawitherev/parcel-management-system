import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { ClaimPayload, ClaimService } from '../../claim-service';
import { FormBuilder, FormGroup, Validators, ɵInternalFormsSharedModule, ReactiveFormsModule } from '@angular/forms';
import { AsyncPipe } from '@angular/common';
import { FormFieldConfig, MyForm } from '../../../../../common/components/form/my-form/my-form';
import { MyButton } from "../../../../../common/components/buttons/my-button/my-button";
import { FileUpload } from '../../../../../common/components/file-upload/file-upload';
import { mapperClaimPayload } from '../../../../../core/bulk-action/excel-to-json';

@Component({
  selector: 'app-claim',
  imports: [ɵInternalFormsSharedModule, ReactiveFormsModule, AsyncPipe, MyForm, MyButton, FileUpload],
  templateUrl: './claim.html',
  styleUrl: './claim.css'
})
export class Claim {
  claimResponse$?: Observable<any>
  formGroup: FormGroup
  isClaiming: boolean = false;
  isBulkClaimPopup: boolean = false;
  payloadMapper: (data: any) => ClaimPayload = mapperClaimPayload

  formFieldConfigs: FormFieldConfig[] = [
    {
      controlName: "trackingNumber", 
      label: "Tracking Number", 
      placeholder: "Enter tracking number", 
      errorMessageV2: {
        required: 'Tracking number is required for claiming'
      }, 
      type: 'text'
    }
  ]

  constructor(private claimService: ClaimService, private fb: FormBuilder) {
    this.formGroup = fb.group({
      trackingNumber: ['', [Validators.required, Validators.maxLength(20)]]
    })
  }

  onClaim(formValue: ClaimPayload) {
    if (this.formGroup.valid) {
      this.claimResponse$ = this.claimService.claimParcel(formValue.trackingNumber);
      this.formGroup.reset();
    }
  }

  onBulkClaimPopup() {
    this.isBulkClaimPopup = !this.isBulkClaimPopup;
  }

  onBulkClaim(trackingNumbers: ClaimPayload[]) {
    this.claimService.bulkClaim(trackingNumbers)
  }
}
