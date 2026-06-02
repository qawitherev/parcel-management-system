import { Injectable } from '@angular/core';

const STORAGE_KEY = 'parcel-management-system-theme';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  private darkMode = false;

  constructor() {
    const stored = localStorage.getItem(STORAGE_KEY) || 'light';
    this.applyTheme(stored);
  }

  toggleMode() {
    const next = this.darkMode ? 'light' : 'dark';
    this.applyTheme(next);
  }

  getIsDarkMode(): boolean {
    return this.darkMode;
  }

  private applyTheme(theme: string) {
    if (theme === 'dark') {
      document.documentElement.setAttribute('data-theme', 'dark');
      this.darkMode = true;
    } else {
      document.documentElement.removeAttribute('data-theme');
      this.darkMode = false;
    }
    localStorage.setItem(STORAGE_KEY, theme);
  }
}
