import { Routes } from '@angular/router';
import { Tracking } from './pages/tracking/tracking';

export const TRACKING_ROUTES: Routes = [
  { path: '', component: Tracking },
  { path: 'searchResult', loadComponent: () => import('./pages/search-result/search-result').then(c => c.SearchResult) }
];
