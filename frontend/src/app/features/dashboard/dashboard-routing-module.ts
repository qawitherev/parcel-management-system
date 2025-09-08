import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardParent } from './pages/dashboard-admin/dashboard-parent';
import { isLoggedInGuard, isAdminAndManagerAuthed } from '../../core/guards/auth-guard-guard';
import { DashboardUser } from './pages/dashboard-user/dashboard-user';

const routes: Routes = [
  { path: 'admin', component: DashboardParent, canActivate:[isLoggedInGuard, isAdminAndManagerAuthed]},
  { path: 'user', component: DashboardUser, canActivate: [isLoggedInGuard]}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
