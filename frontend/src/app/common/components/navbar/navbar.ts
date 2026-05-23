import { Component, OnInit, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NgClass } from '@angular/common';
import { RoleService } from '../../../core/roles/role-service';
import { AuthService } from '../../../features/auth/auth-service';
import { ThemeService } from '../../../core/theme/theme-service';

interface NavGroup {
  label: string;
  route?: string;
  children?: NavGroup[];
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
  openDropdown = signal<string | null>(null);

  navStructure: Record<string, NavGroup[]> = {
    resident: [
      { label: 'Dashboard', route: '/dashboard/user' },
      {
        label: 'Parcels', children: [
          { label: 'Tracking', route: '/parcel/tracking' },
          { label: 'Claim', route: '/parcel/claim' },
          { label: 'All Parcels', route: '/parcel/parcels' },
        ]
      },
      {
        label: 'Settings', children: [
          { label: 'Notifications', route: '/settings/notifications' },
        ]
      },
    ],
    ParcelRoomManager: [
      { label: 'Dashboard', route: '/dashboard/admin' },
      {
        label: 'Operations', children: [
          { label: 'Check In', route: '/parcel/checkIn' },
          { label: 'Tracking', route: '/parcel/tracking' },
          { label: 'All Parcels', route: '/parcel/parcels' },
        ]
      },
      {
        label: 'Management', children: [
          { label: 'Lockers', route: '/locker' },
          { label: 'Units', route: '/residentUnit/units' },
          { label: 'Assignments', route: '/resident/userResidentUnit' },
        ]
      },
      {
        label: 'Settings', children: [
          { label: 'Notifications', route: '/settings/notifications' },
        ]
      },
    ],
    Admin: [
      { label: 'Dashboard', route: '/dashboard/admin' },
      {
        label: 'Operations', children: [
          { label: 'Check In', route: '/parcel/checkIn' },
          { label: 'Claim', route: '/parcel/claim' },
          { label: 'Tracking', route: '/parcel/tracking' },
          { label: 'All Parcels', route: '/parcel/parcels' },
        ]
      },
      {
        label: 'Management', children: [
          { label: 'Lockers', route: '/locker' },
          { label: 'Units', route: '/residentUnit/units' },
          { label: 'Assignments', route: '/resident/userResidentUnit' },
        ]
      },
      {
        label: 'Settings', children: [
          { label: 'Notifications', route: '/settings/notifications' },
        ]
      },
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

  get navGroups(): NavGroup[] {
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

  isDropdownOpen(label: string): boolean {
    return this.openDropdown() === label;
  }

  toggleDropdown(label: string): void {
    this.openDropdown.update(v => v === label ? null : label);
  }

  closeDropdown(): void {
    this.openDropdown.set(null);
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
