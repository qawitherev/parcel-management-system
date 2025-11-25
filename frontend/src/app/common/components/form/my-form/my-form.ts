import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-my-form',
  imports: [],
  templateUrl: './my-form.html',
  styleUrl: './my-form.css'
})
export class MyForm {
 @Input() formGroup!: FormGroup;
 
}
