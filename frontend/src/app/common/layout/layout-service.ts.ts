import { Injectable } from '@angular/core';
import { AppConsole } from '../../utils/app-console';
import { BehaviorSubject, filter, map, Observable } from 'rxjs';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class LayoutService {
  pageTitle$ = new BehaviorSubject<string>('Parcel App');

  constructor(private router: Router, private activatedRoute: ActivatedRoute) {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        map(() => {
          let route = this.activatedRoute;
          while (route.firstChild) {
            route = route.firstChild;
          }
          return route;
        })
      )
      .subscribe((route) => {
        const title = route?.snapshot.data['title'];
        AppConsole.log(`PAGE TITLE: page title is ${title}`);
        if (title) {
          this.pageTitle$.next(title);
        } else {
          this.pageTitle$.next('Parcel App');
        }
      });
  }
}

const PERSISTENT_SIDEBAR_STATE_KEY = 'parcel-management-system-sidebar-state';

@Injectable({
  providedIn: 'root',
})
export class SidebarService {
  getSidebarState(): Map<string, boolean> {
    const savedState = localStorage.getItem(PERSISTENT_SIDEBAR_STATE_KEY);
    return savedState ? new Map(JSON.parse(savedState)) : new Map();
  }

  setSidebarState(state: Map<string, boolean>) {
    const entries = Array.from(state.entries());
    AppConsole.log(entries);
    localStorage.setItem(PERSISTENT_SIDEBAR_STATE_KEY, JSON.stringify(entries));
  }
}
