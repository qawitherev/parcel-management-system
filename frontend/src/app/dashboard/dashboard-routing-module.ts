import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardParent } from './dashboard-parent/dashboard-parent';
import { isLoggedInGuard, isRoleAuthorized } from '../core/guards/auth-guard-guard';

const routes: Routes = [
  { path: '', component: DashboardParent, canActivate:[isLoggedInGuard, isRoleAuthorized]}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
