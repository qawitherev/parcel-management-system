import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup, ReactiveFormsModule, ɵInternalFormsSharedModule } from '@angular/forms';
import { MyButton } from "../../buttons/my-button/my-button";
import { AppConsole } from '../../../../utils/app-console';

export interface FieldConfigValue {
  controlName: string,
  label: string, 
  placeholder: string, 
  invalidMessage: string, 
  type: 'text' | 'password' | 'number'
}

@Component({
  selector: 'app-my-form',
  imports: [ɵInternalFormsSharedModule, ReactiveFormsModule, MyButton],
  templateUrl: './my-form.html',
  styleUrl: './my-form.css'
})
export class MyForm {
 @Input() myFormGroup!: FormGroup;
 @Input() myFields: FieldConfigValue[] = [];
 @Input() submitLabel!: string;

 @Output() submitted = new EventEmitter<any>();

 onSubmit() {
  AppConsole.log(`onSubmit`)
  this.submitted.emit(this.myFormGroup.value);
 }
}
