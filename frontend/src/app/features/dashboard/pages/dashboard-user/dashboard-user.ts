import { Component, OnInit } from '@angular/core';
import { UserAwaitingPickup } from "../../components/user-awaiting-pickup/user-awaiting-pickup";
import { DashboardService, UserResponse } from '../../dashboard-service';
import { Observable, tap } from 'rxjs';
import { ApiError } from '../../../../core/error-handling/api-catch-error';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-dashboard-user',
  imports: [UserAwaitingPickup, AsyncPipe],
  templateUrl: './dashboard-user.html',
  styleUrl: './dashboard-user.css'
})
export class DashboardUser implements OnInit{
  userDetails$?: Observable<UserResponse | ApiError>

  constructor(
    private dashboardService: DashboardService
  ) {}

  ngOnInit(): void {
    this.userDetails$ = this.dashboardService.getUserDetails().pipe(
      tap(res => {
        if (res && 'error' in res) {
          // is error, dont know what to do, 
          // maybe show error message 
        }
      })
    )
  }

}
