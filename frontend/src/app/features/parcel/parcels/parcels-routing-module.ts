import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ParcelsList } from './pages/parcels-list/parcels-list';
import { isLoggedInGuard } from '../../../core/guards/auth-guard-guard';

const routes: Routes = [
  { path: '', component: ParcelsList, canActivate: [isLoggedInGuard]}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ParcelsRoutingModule { }
