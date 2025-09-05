import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Dashboard } from '../dashboard';
import { AsyncPipe, NgIf } from '@angular/common';
@Component({
  selector: 'app-recently-picked-up',
  standalone: true,
  imports: [NgIf, AsyncPipe],
  templateUrl: './recently-picked-up.html',
  styleUrl: './recently-picked-up.css'
})
export class RecentlyPickedUp implements OnInit {
  recentlyPickedUp$?: Observable<any> 

  constructor(private dashboardService: Dashboard) {}

  ngOnInit(): void {
    this.recentlyPickedUp$ = this.dashboardService.getRecentlyPickedUp()
  }
}
