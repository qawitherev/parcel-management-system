import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardParent } from './pages/dashboard-admin/dashboard-parent';
import { isLoggedInGuard, isAdminAndManagerAuthed } from '../../core/guards/auth-guard-guard';

const routes: Routes = [
  { path: '', component: DashboardParent, canActivate:[isLoggedInGuard, isAdminAndManagerAuthed]}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
