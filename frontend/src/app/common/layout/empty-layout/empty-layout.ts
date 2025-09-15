import { Component } from '@angular/core';
import { AuthRoutingModule } from "../../../features/auth/auth-routing-module";
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-empty-layout',
  standalone: true, 
  imports: [AuthRoutingModule],
  templateUrl: './empty-layout.html',
  styleUrl: './empty-layout.css'
})
export class EmptyLayout {

}
