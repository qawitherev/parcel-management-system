import { Routes } from '@angular/router';
import { Unauthorize } from './unauthorize/unauthorize';

export const SYSTEM_PAGES_ROUTES: Routes = [
  { path: '', component: Unauthorize },
  { path: 'uiComponent', loadComponent: () => import('./ui-component/ui-component').then(c => c.UiComponent) }
];
