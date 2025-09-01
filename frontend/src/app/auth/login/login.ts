import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, ɵInternalFormsSharedModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ɵInternalFormsSharedModule, ReactiveFormsModule, RouterModule, NgClass],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  form: FormGroup

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      emailUsername: ['', [Validators.required]], 
      password: ['', [Validators.required]]
    })
  }

  onSubmit() {
    if (this.form.valid) {
      console.log('Login')
    }
  }
}
