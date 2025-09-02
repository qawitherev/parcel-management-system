import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardParent } from './dashboard-parent/dashboard-parent';

const routes: Routes = [
  { path: '', component: DashboardParent}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
