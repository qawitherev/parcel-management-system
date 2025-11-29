import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { NgClass, AsyncPipe } from '@angular/common';
import { Auth } from '../../auth';
import { Observable, tap } from 'rxjs';
import { NgIf } from '@angular/common';
import { AppConsole } from '../../../../utils/app-console';
import { passwordMatchValidator } from '../../../../utils/custom-validators';
import { Router } from '@angular/router';
import { MyButton } from "../../../../common/components/buttons/my-button/my-button";

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, MyButton, AsyncPipe],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  form: FormGroup; 
  errorMessage: string | null = null
  isLoading: boolean = false;
  registerResponse$?: Observable<any>
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
      this.isLoading = true;
      const registerRequest = {
        Username: this.form.value.username,
        Email: this.form.value.email, 
        ResidentUnit: this.form.value.residentUnit,
        Password: this.form.value.password
      }
      this.registerResponse$ = this.authService.register(registerRequest).pipe(
        tap(res => {
          if (res && 'error' in res) {
            this.errorMessage = res.message;
          } else {
            this.router.navigateByUrl('/login')
          }
          this.isLoading = false;
        })
      )
    } else {
      AppConsole.log('Register clicked!')
    }
  }
}
