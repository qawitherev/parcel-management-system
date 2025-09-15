import { Component, OnInit } from '@angular/core';
import { AuthRoutingModule } from "../../../features/auth/auth-routing-module";
import { SidebarService } from '../layout-service.ts';

interface MenuItem {
  label: string, 
  route?: string, 
  children?: MenuItem[]
}

const MENU_ITEMS: MenuItem[] = [
  {
    label: 'Dashboard', 
    route: '/dashboard/user'
  }, 
  {
    label: 'Parcel', 
    children: [
      {
        label: 'CheckIn', 
        route: '/parcel/checkIn'
      }, 
      {
        label: 'Tracking', 
        route: '/parcel/tracking'
      }, 
      {
        label: 'Claim', 
        route: '/parcel/claim'
      }, 
      {
        label: 'All Parcels', 
        route: '/parcel/parcels'
      }
    ]
  }
]


@Component({
  selector: 'app-normal-layout',
  imports: [AuthRoutingModule],
  templateUrl: './normal-layout.html',
  styleUrl: './normal-layout.css'
})
export class NormalLayout implements OnInit {

  constructor(private sidebarService: SidebarService) {}

  menuItems = MENU_ITEMS
  expandedMap = new Map<string, boolean>()

    ngOnInit(): void {
    this.expandedMap = this.sidebarService.getSidebarState()
  }

  toggleParentExpand(label: string) {
    const current = this.expandedMap.get(label) ?? false 
    this.expandedMap.set(label, !current)
    this.sidebarService.setSidebarState(this.expandedMap)
  }

  isExpanded(label: string): boolean {
    return this.expandedMap.get(label) ?? false
  }
}
