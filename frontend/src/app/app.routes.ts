import { Routes } from '@angular/router';
import { EmptyLayout } from './common/layout/empty-layout/empty-layout';
import { NormalLayout } from './common/layout/normal-layout/normal-layout';
import { isAdminAndManagerAuthed, isLoggedInGuard } from './core/guards/auth-guard-guard';

export const routes: Routes = [
  // Auth (no chrome)
  {
    path: '',
    component: EmptyLayout,
    children: [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      { path: '', loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES) }
    ]
  },

  // System pages (no chrome)
  {
    path: 'systemPages',
    component: EmptyLayout,
    children: [
      { path: '', loadChildren: () => import('./system-pages/system-pages.routes').then(m => m.SYSTEM_PAGES_ROUTES) }
    ]
  },

  // Parcel (with chrome)
  {
    path: 'parcel',
    component: NormalLayout,
    children: [
      { path: 'tracking', loadChildren: () => import('./features/parcel/tracking/tracking.routes').then(m => m.TRACKING_ROUTES), data: { title: 'Parcel Tracking' } },
      { path: 'checkIn', loadChildren: () => import('./features/parcel/check-in/check-in.routes').then(m => m.CHECK_IN_ROUTES), data: { title: 'Check In' } },
      { path: 'claim', loadChildren: () => import('./features/parcel/claim/claim.routes').then(m => m.CLAIM_ROUTES), data: { title: 'Parcel Claim' } },
      { path: 'parcels', loadChildren: () => import('./features/parcel/parcels/parcels.routes').then(m => m.PARCELS_ROUTES), data: { title: 'All Parcels' } }
    ],
    canActivate: [isLoggedInGuard]
  },

  // Dashboard (with chrome)
  {
    path: 'dashboard',
    component: NormalLayout,
    children: [
      { path: '', loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES), data: { title: 'Dashboard' } }
    ]
  },

  // Resident (with chrome)
  {
    path: 'resident',
    component: NormalLayout,
    children: [
      { path: 'userResidentUnit', loadChildren: () => import('./features/resident/user-resident-unit/user-resident-unit.routes').then(m => m.USER_RESIDENT_UNIT_ROUTES), data: { title: 'User Resident Unit' } }
    ],
    canActivate: [isLoggedInGuard]
  },

  // Resident Units (with chrome)
  {
    path: 'residentUnit',
    component: NormalLayout,
    children: [
      { path: 'units', loadChildren: () => import('./features/resident-units/units/units.routes').then(m => m.UNITS_ROUTES), data: { title: 'Resident Units' } }
    ],
    canActivate: [isLoggedInGuard]
  },

  // Locker (with chrome)
  {
    path: 'locker',
    component: NormalLayout,
    children: [
      { path: '', loadChildren: () => import('./features/locker/locker.routes').then(m => m.LOCKER_ROUTES), data: { title: 'Locker' } }
    ],
    canActivate: [isLoggedInGuard, isAdminAndManagerAuthed]
  },

  // Settings (with chrome)
  {
    path: 'settings',
    component: NormalLayout,
    children: [
      { path: 'notifications', loadChildren: () => import('./features/system-settings/notification-prefs/notification-prefs.routes').then(m => m.NOTIFICATION_PREFS_ROUTES), data: { title: 'Notifications' }, canActivate: [isLoggedInGuard] }
    ]
  }
];
