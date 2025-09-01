import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, ɵInternalFormsSharedModule } from '@angular/forms';
import { RouterModule } from '@angular/router'; 

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ɵInternalFormsSharedModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  form: FormGroup

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      emailUsername: [''], 
      password: ['']
    })
  }

  onSubmit() {
    if (this.form.valid) {
      console.log('Login')
    }
  }
}
