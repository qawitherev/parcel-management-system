import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Tracking } from './pages/tracking/tracking';
import { isLoggedInGuard } from '../../../core/guards/auth-guard-guard';

const routes: Routes = [
  { path: '', component: Tracking, canActivate: [isLoggedInGuard]}, 
  { path: 'searchResult', loadComponent: () => import('./pages/search-result/search-result')
      .then(c => c.SearchResult), canActivate: [isLoggedInGuard]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TrackingRoutingModule { }
