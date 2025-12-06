import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup, ReactiveFormsModule, ɵInternalFormsSharedModule } from '@angular/forms';
import { MyButton } from "../../buttons/my-button/my-button";
import { AppConsole } from '../../../../utils/app-console';

export interface FormFieldConfig {
  controlName: string,
  label: string, 
  placeholder: string, 
  errorMessage?: { [key: string]: string}
  errorMessageV2?: Partial<Record<Validator, string>>,
  type: 'text' | 'password' | 'number', 
}

type Validator = 'required' | 'min' | 'max' | 'minlength' | 'maxlength'

@Component({
  selector: 'app-my-form',
  imports: [ɵInternalFormsSharedModule, ReactiveFormsModule, MyButton],
  templateUrl: './my-form.html',
  styleUrl: './my-form.css'
})
export class MyForm {
 @Input() myFormGroup!: FormGroup;
 @Input() myFields: FormFieldConfig[] = [];
 @Input() submitLabel!: string;

 @Output() submitted = new EventEmitter<any>();

 onSubmit() {
  AppConsole.log(`onSubmit`)
  this.submitted.emit(this.myFormGroup.value);
 }

 getErrorMessage(controlName: string): string {
  /**
   * get the controlName 
   * get the validatorName 
   * combination ==> the right error message 
   */

  const theControl = this.myFormGroup.get(controlName);
  let validator: Validator;
  if (theControl && theControl.errors) {
    validator = Object.keys(theControl.errors)[0] as Validator;
    const field = this.myFields.find(f => f.controlName === controlName);
    const errMessage = field?.errorMessageV2?.[validator] || '';
    return errMessage; 
  }
  return 'This field has error'
 }
}
