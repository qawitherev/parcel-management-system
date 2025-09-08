import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AppConsole } from '../../../../../utils/app-console';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-tracking',
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './tracking.html',
  styleUrl: './tracking.css'
})
export class Tracking {
  formGroup: FormGroup

  constructor(private router: Router, private route: ActivatedRoute,
      private fb: FormBuilder
  ) {
    this.formGroup = fb.group({
      searchKeyword: ['', [Validators.required]]
    })
  }

  onSubmit() {
    if(this.formGroup.valid) {
      AppConsole.log(`search...`)
      this.router.navigate(['searchResult'], { relativeTo: this.route })
    }
  }
}
