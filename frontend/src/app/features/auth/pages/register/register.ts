import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { NgClass, AsyncPipe } from '@angular/common';
import { Auth } from '../../auth';
import { Observable } from 'rxjs';
import { NgIf } from '@angular/common';
import { AppConsole } from '../../../../utils/app-console';
import { passwordMatchValidator } from '../../../../utils/custom-validators';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, NgClass, NgIf],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  form: FormGroup; 
  errorMessage: string | null = null
  // registerResponse$?: Observable<any>


  constructor(private fb: FormBuilder, private authService: Auth, private router: Router) {
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
      this.authService.register(registerRequest).subscribe(
        {
          next: (res) => {
            if(res.error) {
              this.errorMessage = res.message
            } else {
              this.router.navigateByUrl('/login')
            }
          }
        }
      )
    } else {
      AppConsole.log('Register clicked!')
    }
  }
}
