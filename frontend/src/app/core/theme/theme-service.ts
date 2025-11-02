import { Injectable } from '@angular/core';
import { AppConsole } from '../../utils/app-console';

const PERSISTENT_THEME_MODE = "parcel-management-system-theme-state"
const PERSISTENT_THEME_MODE_DARK = "parcel-management-system-theme-state-dark"
const PERSISTENT_THEME_MODE_LIGHT = "parcel-management-system-theme-state-light"

@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  private darkMode = false;

  constructor() {
    // get from local storage 
    const themeState = localStorage.getItem(PERSISTENT_THEME_MODE);
    if (themeState == PERSISTENT_THEME_MODE_DARK) {
      this.turnToDark();
      this.darkMode = true;
    } else {
      localStorage.setItem(PERSISTENT_THEME_MODE, PERSISTENT_THEME_MODE_LIGHT);
      this.darkMode = false;
      this.turnToLight();
    }
  }

  turnToLight() {
    document.documentElement.classList.remove('dark-theme');
    localStorage.setItem(PERSISTENT_THEME_MODE, PERSISTENT_THEME_MODE_LIGHT);
    this.darkMode = false;
  }

  turnToDark() {
    document.documentElement.classList.add('dark-theme');
    localStorage.setItem(PERSISTENT_THEME_MODE, PERSISTENT_THEME_MODE_DARK);
    this.darkMode = true;
  }

  toggleMode() {
    this.darkMode ? this.turnToLight() : this.turnToDark();
  }

  getIsDarkMode() : boolean {
    return this.darkMode;
  }
}
