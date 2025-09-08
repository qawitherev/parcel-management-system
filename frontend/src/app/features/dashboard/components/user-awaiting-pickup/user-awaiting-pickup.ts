import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Dashboard } from '../../dashboard';
import { NgIf, AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-user-awaiting-pickup',
  imports: [NgIf, AsyncPipe],
  templateUrl: './user-awaiting-pickup.html',
  styleUrl: './user-awaiting-pickup.css'
})

export class UserAwaitingPickup implements OnInit {
  parcelsAwaitingPickup$?: Observable<any>

  constructor(private dashboardService: Dashboard) {}

  ngOnInit(): void {
    this.parcelsAwaitingPickup$ = this.dashboardService.getUserAwaitingPickup()
  }

  
}
