import { Routes } from '@angular/router';
import { DashboardParent } from './pages/dashboard-admin/dashboard-parent';
import { DashboardUser } from './pages/dashboard-user/dashboard-user';
import { isLoggedInGuard, isAdminAndManagerAuthed } from '../../core/guards/auth-guard-guard';

export const DASHBOARD_ROUTES: Routes = [
  { path: 'admin', component: DashboardParent, canActivate: [isLoggedInGuard, isAdminAndManagerAuthed] },
  { path: 'user', component: DashboardUser, canActivate: [isLoggedInGuard] }
];
