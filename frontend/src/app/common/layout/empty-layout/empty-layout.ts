import { Component } from '@angular/core';
import { AuthRoutingModule } from "../../../features/auth/auth-routing-module";
import { RouterOutlet } from '@angular/router';
import { ThemeService } from '../../../core/theme/theme-service';
import { NgClass } from '@angular/common';
import { MySwitch } from "../../components/switch/my-switch/my-switch";

@Component({
  selector: 'app-empty-layout',
  standalone: true, 
  imports: [AuthRoutingModule, NgClass, MySwitch],
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

  get isDarkMode() : boolean {
    return this.themeService.getIsDarkMode();
  }

}
