import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, ɵInternalFormsSharedModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgClass, AsyncPipe } from '@angular/common';
import { Observable } from 'rxjs';
import { Auth } from '../../auth';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ɵInternalFormsSharedModule, ReactiveFormsModule, RouterModule, NgClass, NgIf, AsyncPipe],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  form: FormGroup
  loginResponse$?: Observable<any>

  constructor(private fb: FormBuilder, private authService: Auth) {
    this.form = this.fb.group({
      emailUsername: ['', [Validators.required]], 
      password: ['', [Validators.required]]
    })
  }

  onSubmit() {
    if (this.form.valid) {
      const loginRequest = {
        Username: this.form.value.emailUsername,
        PlainPassword: this.form.value.password
      }
      this.loginResponse$ = this.authService.login(loginRequest)
    }
  }
}
