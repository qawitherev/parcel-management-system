import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { ClaimService } from '../../claim-service';
import { FormBuilder, FormGroup, Validators, ɵInternalFormsSharedModule, ReactiveFormsModule } from '@angular/forms';
import { AsyncPipe } from '@angular/common';
import { AppConsole } from '../../../../../utils/app-console';
import { NgIf } from '@angular/common';
import {
  ButtonComponent,
  InputComponent,
  CardComponent,
  CardHeaderComponent,
  CardTitleComponent,
  CardDescriptionComponent,
  CardContentComponent,
  LabelComponent,
  AlertComponent,
  AlertDescriptionComponent,
} from '../../../../../components/ui';

@Component({
  selector: 'app-claim',
  imports: [
    ɵInternalFormsSharedModule,
    ReactiveFormsModule,
    NgIf,
    AsyncPipe,
    ButtonComponent,
    InputComponent,
    CardComponent,
    CardHeaderComponent,
    CardTitleComponent,
    CardDescriptionComponent,
    CardContentComponent,
    LabelComponent,
    AlertComponent,
    AlertDescriptionComponent,
  ],
  templateUrl: './claim.html',
  styleUrl: './claim.css'
})
export class Claim {
  claimResponse$?: Observable<any>
  formGroup: FormGroup

  constructor(private claimService: ClaimService, private fb: FormBuilder) {
    this.formGroup = fb.group({
      trackingNumber: ['', [Validators.required, Validators.maxLength(20)]]
    })
  }

  onClaim() {
    AppConsole.log(`onClaim()`)
    if (this.formGroup.valid) {
      var trackingNum = this.formGroup.value.trackingNumber
      this.claimResponse$ = this.claimService.claimParcel(trackingNum)
    }
  }
}
