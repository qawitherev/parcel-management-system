import { Component, input, OnInit } from '@angular/core';
import { ResidentUnit, UnitsService } from '../../units-service';
import { Observable, switchMap } from 'rxjs';
import { ApiError } from '../../../../../core/error-handling/api-catch-error';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-units-edit',
  standalone: true, 
  imports: [],
  templateUrl: './units-edit.html',
  styleUrl: './units-edit.css'
})
export class UnitsEdit implements OnInit {

  theUnit$? : Observable<ResidentUnit | ApiError>
  constructor(private unitService: UnitsService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.theUnit$ = this.route.params.pipe(
      switchMap(param => this.unitService.getUnit(param['id']))
    )
  }
}
