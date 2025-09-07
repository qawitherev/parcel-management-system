import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { UserDashboardService } from '../user-dashboard-service';
import { NgIf, AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-awaiting-pickup',
  imports: [NgIf, AsyncPipe],
  templateUrl: './awaiting-pickup.html',
  styleUrl: './awaiting-pickup.css'
})
export class AwaitingPickup implements OnInit {
  awaitingPickup$?: Observable<any>

  constructor(private dashboardService: UserDashboardService) {}

  ngOnInit(): void {
    this.awaitingPickup$ = this.dashboardService.getUserAwaitingPickup()
 }
}
