import { Injectable } from '@angular/core';
import { AppConsole } from '../../utils/app-console';

@Injectable({
  providedIn: 'root'
})
export class LayoutServiceTs {
  
}

const PERSISTENT_SIDEBAR_STATE_KEY = 'parcel-management-system-sidebar-state'

@Injectable({
  providedIn: 'root'
})
export class SidebarService {

  getSidebarState(): Map<string, boolean> {
    const savedState = localStorage.getItem(PERSISTENT_SIDEBAR_STATE_KEY)
    return savedState ? new Map(JSON.parse(savedState)) : new Map()
  }

  setSidebarState(state: Map<string, boolean>) {
    const entries = Array.from(state.entries())
    AppConsole.log(entries)
    localStorage.setItem(PERSISTENT_SIDEBAR_STATE_KEY, JSON.stringify(entries))
  }
}
