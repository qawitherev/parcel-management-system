import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ɵInternalFormsSharedModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { passwordMatchValidator } from '../../utils/custom-validators';
import { Auth } from '../auth';
import { ReactiveFormsModule } from '@angular/forms';
import { NgIf, AsyncPipe, NgClass } from '@angular/common';
import { AppConsole } from '../../utils/app-console';
@Component({
  selector: 'app-register-manager',
  imports: [ReactiveFormsModule, NgIf, AsyncPipe, NgClass],
  templateUrl: './register-manager.html',
  styleUrl: './register-manager.css'
})
export class RegisterManager {
  formGroup: FormGroup
  registerResponse$?: Observable<any>

  constructor(private fb: FormBuilder, private authService: Auth) {
    this.formGroup = fb.group({
      username: ['', [Validators.required]], 
      email: ['', [Validators.required]], 
      password: ['', [Validators.required]], 
      confirmPassword: ['', [Validators.required]]
    }, 
    {validators: [passwordMatchValidator]}
  )
  }
  onSubmit() {
    if(this.formGroup.valid) {
      AppConsole.log('Registering manager')
      const payload = {
        Username: this.formGroup.value.username, 
        Email: this.formGroup.value.email, 
        Password: this.formGroup.value.password
      }
      this.registerResponse$ = this.authService.registerManager(payload)
    }
  }
}
