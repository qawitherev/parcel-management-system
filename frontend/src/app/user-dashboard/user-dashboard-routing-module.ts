import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserDashboardParent } from './user-dashboard-parent/user-dashboard-parent';
import { isLoggedInGuard } from '../core/guards/auth-guard-guard';

const routes: Routes = [
  {path: '', component: UserDashboardParent, canActivate:[isLoggedInGuard] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserDashboardRoutingModule { }
