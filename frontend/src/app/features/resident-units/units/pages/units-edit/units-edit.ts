import { Component, input, OnInit } from '@angular/core';
import { ResidentUnit, UnitsService } from '../../units-service';
import { Observable, switchMap, tap } from 'rxjs';
import { ApiError } from '../../../../../core/error-handling/api-catch-error';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-units-edit',
  standalone: true, 
  imports: [],
  templateUrl: './units-edit.html',
  styleUrl: './units-edit.css'
})
export class UnitsEdit implements OnInit {
  formGroup?: FormGroup
  theUnit$? : Observable<ResidentUnit | ApiError>
  id?: string

  constructor(private unitService: UnitsService, private route: ActivatedRoute, private fb: FormBuilder) {
    this.formGroup = fb.group({
      unitName: ['', [Validators.required]]
    })
  }

  ngOnInit(): void {
    this.theUnit$ = this.route.params.pipe(
      switchMap(param => this.unitService.getUnit(param['id'])), 
      tap(
        unit => {
          if ('unitName' in unit) {
            this.formGroup?.patchValue({
              unitName: unit.unitName
            })
            this.id = unit.id
          }
        }
      )
    )
  }

  onUpdate(): void {
    if (this.formGroup?.valid && this.id != null) {
      this.unitService.updateUnit(this.id, this.formGroup.get('unitName')?.value)
    }
  }


}
