import { Component } from '@angular/core';
import { UserAwaitingPickup } from "../../components/user-awaiting-pickup/user-awaiting-pickup";

@Component({
  selector: 'app-dashboard-user',
  imports: [UserAwaitingPickup],
  templateUrl: './dashboard-user.html',
  styleUrl: './dashboard-user.css'
})
export class DashboardUser {

}
