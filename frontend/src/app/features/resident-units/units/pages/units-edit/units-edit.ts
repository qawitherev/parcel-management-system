import { Component, input, OnInit } from '@angular/core';
import { ResidentUnit, UnitsService } from '../../units-service';
import { Observable, switchMap, tap } from 'rxjs';
import { ApiError } from '../../../../../core/error-handling/api-catch-error';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';

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

  constructor(private unitService: UnitsService, private route: ActivatedRoute, private fb: FormBuilder) {
    this.formGroup = fb.group({
      unitName: []
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
          }
        }
      )
    )
  }


}
