import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Dashboard } from '../../dashboard';
import { AsyncPipe, NgIf } from '@angular/common';
import { AppConsole } from '../../../../utils/app-console';

@Component({
  selector: 'app-awaiting-pickup',
  standalone: true,
  imports: [NgIf, AsyncPipe],
  templateUrl: './awaiting-pickup.html',
  styleUrl: './awaiting-pickup.css'
})

export class AwaitingPickup implements OnInit {
  awaitingPickup$?: Observable<any>

  constructor(private dashboardService: Dashboard) {}

  ngOnInit(): void {
    this.awaitingPickup$ = this.dashboardService.getAwaitingPickup()
    AppConsole.log(this.awaitingPickup$)
  }
}
