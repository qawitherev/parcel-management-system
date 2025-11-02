import { Component } from '@angular/core';
import { AuthRoutingModule } from "../../../features/auth/auth-routing-module";
import { RouterOutlet } from '@angular/router';
import { ThemeService } from '../../../core/theme/theme-service';

@Component({
  selector: 'app-empty-layout',
  standalone: true, 
  imports: [AuthRoutingModule],
  templateUrl: './empty-layout.html',
  styleUrl: './empty-layout.css'
})
export class EmptyLayout {
  constructor(
    private themeService: ThemeService
  ) {}

  onThemeToggle() {
    this.themeService.toggleMode();
  }
}
