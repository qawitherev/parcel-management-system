import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { NgClass, AsyncPipe } from '@angular/common';
import { Auth } from '../auth';
import { Observable } from 'rxjs';
import { NgIf } from '@angular/common';
import { AppConsole } from '../../utils/app-console';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value
  const confirmPassword = control.get('confirmPassword')?.value
  return password === confirmPassword ? null : 
    {passwordMismatch: true, errorString: 'Passwords do not match'}
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass, NgIf, AsyncPipe],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  form: FormGroup; 
  registerResponse$?: Observable<any>

  constructor(private fb: FormBuilder, private authService: Auth) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.maxLength(10)]],
      residentUnit: ['RU001', [Validators.required, Validators.maxLength(10)]],
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
      const registerRequest = {
        Username: this.form.value.username,
        Email: this.form.value.email, 
        ResidentUnit: this.form.value.residentUnit,
        Password: this.form.value.password
      }
      this.registerResponse$ = this.authService.register(registerRequest)
    } else {
      AppConsole.log('Register clicked!')
    }
  }
}
