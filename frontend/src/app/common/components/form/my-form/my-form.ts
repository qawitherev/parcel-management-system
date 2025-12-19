import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup, ReactiveFormsModule, ɵInternalFormsSharedModule } from '@angular/forms';
import { MyButton } from '../../buttons/my-button/my-button';

export interface FormFieldConfig {
  controlName: string;
  label: string;
  placeholder: string;
  errorMessage?: { [key: string]: string };
  errorMessageV2?: Partial<Record<Validator, string>>;
  type: 'text' | 'password' | 'number';
}

type Validator = 'required' | 'min' | 'max' | 'minlength' | 'maxlength';

@Component({
  selector: 'app-my-form',
  imports: [ɵInternalFormsSharedModule, ReactiveFormsModule, MyButton],
  templateUrl: './my-form.html',
  styleUrl: './my-form.css',
})
export class MyForm<T = any> {
  @Input() myFormGroup!: FormGroup;
  @Input() myFields: FormFieldConfig[] = [];
  @Input() submitLabel!: string;
  @Input() isSubmitLoading!: boolean;

  @Output() submitted = new EventEmitter<T>();

  onSubmit() {
    if (this.myFormGroup.valid) {
      this.submitted.emit(this.myFormGroup.value as T);
    }
  }

  getErrorMessage(controlName: string): string {
    const theControl = this.myFormGroup.get(controlName);
    let validator: Validator;
    if (theControl && theControl.errors) {
      validator = Object.keys(theControl.errors)[0] as Validator;
      const field = this.myFields.find((f) => f.controlName === controlName);
      const errMessage = field?.errorMessageV2?.[validator] || '';
      return errMessage;
    }
    return 'This field has error';
  }
}
