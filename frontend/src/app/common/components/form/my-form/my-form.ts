import { Component, Input } from '@angular/core';
import { FormGroup, ReactiveFormsModule, ɵInternalFormsSharedModule } from '@angular/forms';
import { MyButton } from "../../buttons/my-button/my-button";

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

}
