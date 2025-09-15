import { Component, OnInit } from '@angular/core';
import { AuthRoutingModule } from '../../../features/auth/auth-routing-module';
import { SidebarService } from '../layout-service.ts';
import { RoleService } from '../../../core/roles/role-service';
import { AppConsole } from '../../../utils/app-console';
import { BehaviorSubject } from 'rxjs';
import { AsyncPipe, NgIf } from '@angular/common';

interface MenuItem {
  label: string;
  route?: string;
  roles?: string[];
  children?: MenuItem[];
}

const MENU_ITEMS: MenuItem[] = [
  {
    label: 'Dashboard',
    route: '/dashboard/user',
  },
  {
    label: 'Parcel',
    children: [
      {
        label: 'CheckIn',
        route: '/parcel/checkIn',
        roles: ['ParcelRoomManager'],
      },
      {
        label: 'Tracking',
        route: '/parcel/tracking',
        roles: ['Resident', 'ParcelRoomManager', 'Admin'],
      },
      {
        label: 'Claim',
        route: '/parcel/claim',
        roles: ['Resident', 'Admin'],
      },
      {
        label: 'All Parcels',
        route: '/parcel/parcels',
        roles: ['Resident', 'ParcelRoomManager', 'Admin'],
      },
    ],
  },
  {
    label: 'Resident',
    children: [
      {
        label: 'User Resident Unit',
        route: '/resident/userResidentUnit',
      },
    ],
  },
];

@Component({
  selector: 'app-normal-layout',
  imports: [AuthRoutingModule, NgIf, AsyncPipe],
  templateUrl: './normal-layout.html',
  styleUrl: './normal-layout.css',
})
export class NormalLayout implements OnInit {
  constructor(private roleService: RoleService, private sidebarService: SidebarService) {}

  menuItems = MENU_ITEMS;
  expandedMap = new Map<string, boolean>();
  userRole?: string;
  userRoleStream$ = new BehaviorSubject<string | null>(null)

  ngOnInit(): void {
    this.expandedMap = this.sidebarService.getSidebarState();
    this.roleService.getRole().subscribe((role) => {
      this.userRole = role?.role ?? '';
      this.userRoleStream$.next(role?.role ?? null)
    });
  }

  toggleParentExpand(label: string) {
    const current = this.expandedMap.get(label) ?? false;
    this.expandedMap.set(label, !current);
    this.sidebarService.setSidebarState(this.expandedMap);
  }

  isExpanded(label: string): boolean {
    return this.expandedMap.get(label) ?? false;
  }
}
