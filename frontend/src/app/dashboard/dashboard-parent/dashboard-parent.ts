import { Component } from '@angular/core';
import { AwaitingPickup } from '../awaiting-pickup/awaiting-pickup';
import { RecentlyPickedUp } from '../recently-picked-up/recently-picked-up';

@Component({
  selector: 'app-dashboard-parent',
  standalone: true,
  imports: [AwaitingPickup, RecentlyPickedUp],
  templateUrl: './dashboard-parent.html',
  styleUrl: './dashboard-parent.css'
})
export class DashboardParent {

}
