import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { ClaimService } from '../../claim-service';
import { FormBuilder, FormGroup, Validators, ɵInternalFormsSharedModule, ReactiveFormsModule } from '@angular/forms';
import { AsyncPipe } from '@angular/common';
import { AppConsole } from '../../../../../utils/app-console';
import { FormFieldConfig, MyForm } from '../../../../../common/components/form/my-form/my-form';

interface ClaimFormOutput {
  trackingNumber: string
}

@Component({
  selector: 'app-claim',
  imports: [ɵInternalFormsSharedModule, ReactiveFormsModule, AsyncPipe, MyForm],
  templateUrl: './claim.html',
  styleUrl: './claim.css'
})
export class Claim {
  claimResponse$?: Observable<any>
  formGroup: FormGroup
  isClaiming: boolean = false;

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

  onClaim(formValue: ClaimFormOutput) {
    AppConsole.log(`${JSON.stringify(formValue)}`)
    if (this.formGroup.valid) {
      this.claimResponse$ = this.claimService.claimParcel(formValue.trackingNumber);
      this.formGroup.reset();
    }
  }
}
