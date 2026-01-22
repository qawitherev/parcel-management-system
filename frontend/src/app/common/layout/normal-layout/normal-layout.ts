import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthRoutingModule } from '../../../features/auth/auth-routing-module';
import { LayoutService, SidebarService } from '../layout-service.ts';
import { RoleService, RoleWithExp } from '../../../core/roles/role-service';
import { BehaviorSubject, combineLatest, map, Observable, pipe, Subject, takeUntil } from 'rxjs';
import { AsyncPipe, NgIf } from '@angular/common';
import { AuthService } from '../../../features/auth/auth-service';
import { MyButton } from "../../components/buttons/my-button/my-button";
import { ThemeService } from '../../../core/theme/theme-service';
import { MySwitch } from "../../components/switch/my-switch/my-switch";

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
    roles: ['Resident', 'ParcelRoomManager', 'Admin']
  },
  {
    label: 'Parcel',
    roles: ['Resident', 'ParcelRoomManager', 'Admin'],
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
    roles: ['ParcelRoomManager', 'Admin'],
    children: [
      {
        label: 'User Resident Unit',
        route: '/resident/userResidentUnit',
        roles: ['ParcelRoomManager', 'Admin'],
      },
    ],
  },
  {
    label: 'Resident Unit', 
    roles: ['ParcelRoomManager', 'Admin'], 
    children: [
      {
        label: 'Resident Units', 
        route: '/residentUnit/units',
        roles: ['ParcelRoomManager', 'Admin']
      }
    ]
  }, 
  {
    label: 'Locker', 
    roles: ['ParcelRoomManager', 'Admin'], 
    children: [
      {
        label: 'Lockers', 
        route: '/locker',
        roles: ['ParcelRoomManager', 'Admin']
      }
    ]
  }, 
  {
    label: 'Settings', 
    roles: ['Resident'], 
    children: [
      {
        label: 'Notifications', 
        route: '/settings/notifications', 
        roles: ['Resident']
      }
    ]
  }
];

@Component({
  selector: 'app-normal-layout',
  imports: [AuthRoutingModule, NgIf, AsyncPipe, MyButton, MySwitch],
  templateUrl: './normal-layout.html',
  styleUrl: './normal-layout.css',
})
export class NormalLayout implements OnInit, OnDestroy {
  constructor(
    private layoutService: LayoutService,
    private roleService: RoleService,
    private sidebarService: SidebarService, 
    private authService: AuthService, 
    private themeService: ThemeService
  ) {}

  menuItems = MENU_ITEMS;
  expandedMap = new Map<string, boolean>();
  userRole$?: Observable<RoleWithExp | null>;
  pageTitle$?: Observable<string>
  private destroy$ = new Subject<void>()

  ngOnInit(): void {
    this.expandedMap = this.sidebarService.getSidebarState()
    this.userRole$ = this.roleService.getRole().pipe(
      map(r => r ?? null), 
      takeUntil(this.destroy$)
    )
    this.pageTitle$ = this.layoutService.pageTitle$.pipe(
      map(title => title),
      takeUntil(this.destroy$)
    )
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  toggleParentExpand(label: string) {
    const current = this.expandedMap.get(label) ?? false;
    this.expandedMap.set(label, !current);
    this.sidebarService.setSidebarState(this.expandedMap);
  }

  isExpanded(label: string): boolean {
    return this.expandedMap.get(label) ?? false;
  }

  logout() {
    this.authService.logout()
  }

  get isDarkMode(): boolean {
    return this.themeService.getIsDarkMode();
  }

  onThemeToggle() {
    this.themeService.toggleMode();
  }
}
