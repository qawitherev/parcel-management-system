import { Component } from '@angular/core';
import { AwaitingPickup } from "../awaiting-pickup/awaiting-pickup";

@Component({
  selector: 'app-user-dashboard-parent',
  standalone: true,
  imports: [AwaitingPickup],
  templateUrl: './user-dashboard-parent.html',
  styleUrl: './user-dashboard-parent.css'
})
export class UserDashboardParent {

}
