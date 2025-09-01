import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { NgClass } from '@angular/common';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value
  const confirmPassword = control.get('confirmPassword')?.value
  return password === confirmPassword ? null : 
    {passwordMismatch: true, errorString: 'Passwords do not match'}
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  form: FormGroup; 

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(10)]],
      password: ['', [Validators.required]],
      confirmPassword: ['', [Validators.required]], 
      agreeTerms: [false]
    }, 
    // Pass options as the second argument, but use the new object signature:
    { validators: [passwordMatchValidator] }
    );
  }

  onSubmit() {
    if (this.form.valid) {
      console.log(this.form.value)
    } else {
      console.info('Register clicked!')
    }
  }
}
