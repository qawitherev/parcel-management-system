import { Component, OnInit, OnDestroy, signal, effect } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NgClass } from '@angular/common';
import { RoleService } from '../../../core/roles/role-service';
import { AuthService } from '../../../features/auth/auth-service';
import { ThemeService } from '../../../core/theme/theme-service';

interface NavItem {
  label?: string;
  route?: string;
  section?: string;
}

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, NgClass],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class Navbar implements OnInit {
  currentRole = signal<string>('resident');
  mobileOpen = signal<boolean>(false);
  activeRoute = signal<string>('');

  navStructure: Record<string, NavItem[]> = {
    resident: [
      { label: 'Dashboard', route: '/dashboard/user' },
      { section: 'Parcels' },
      { label: 'Tracking', route: '/parcel/tracking' },
      { label: 'Claim', route: '/parcel/claim' },
      { label: 'All Parcels', route: '/parcel/parcels' },
      { section: 'Settings' },
      { label: 'Notifications', route: '/settings/notifications' },
    ],
    ParcelRoomManager: [
      { label: 'Dashboard', route: '/dashboard/admin' },
      { section: 'Operations' },
      { label: 'Check In', route: '/parcel/checkIn' },
      { label: 'Tracking', route: '/parcel/tracking' },
      { label: 'All Parcels', route: '/parcel/parcels' },
      { section: 'Management' },
      { label: 'Lockers', route: '/locker' },
      { label: 'Units', route: '/residentUnit/units' },
      { label: 'Assignments', route: '/resident/userResidentUnit' },
      { section: 'Settings' },
      { label: 'Notifications', route: '/settings/notifications' },
    ],
    Admin: [
      { label: 'Dashboard', route: '/dashboard/admin' },
      { section: 'Operations' },
      { label: 'Check In', route: '/parcel/checkIn' },
      { label: 'Claim', route: '/parcel/claim' },
      { label: 'Tracking', route: '/parcel/tracking' },
      { label: 'All Parcels', route: '/parcel/parcels' },
      { section: 'Management' },
      { label: 'Lockers', route: '/locker' },
      { label: 'Units', route: '/residentUnit/units' },
      { label: 'Assignments', route: '/resident/userResidentUnit' },
      { section: 'Settings' },
      { label: 'Notifications', route: '/settings/notifications' },
    ],
  };

  constructor(
    private roleService: RoleService,
    private authService: AuthService,
    private themeService: ThemeService,
  ) {}

  ngOnInit(): void {
    this.roleService.getRole().subscribe(r => {
      if (r && r.role) {
        this.currentRole.set(r.role);
      }
    });
  }

  get navItems(): NavItem[] {
    return this.navStructure[this.currentRole()] || this.navStructure['resident'];
  }

  get roleDisplayName(): string {
    const role = this.currentRole();
    if (role === 'ParcelRoomManager') return 'Manager';
    if (role === 'Admin') return 'Admin';
    return 'Resident';
  }

  get isDark(): boolean {
    return this.themeService.getIsDarkMode();
  }

  toggleTheme(): void {
    this.themeService.toggleMode();
  }

  toggleMobile(): void {
    this.mobileOpen.update(v => !v);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
  }

  logout(): void {
    this.authService.logout();
  }
}
