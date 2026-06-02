import { Routes } from '@angular/router';
import { Units } from './pages/units/units';

export const UNITS_ROUTES: Routes = [
  { path: '', component: Units },
  { path: 'edit/:id', loadComponent: () => import('./pages/units-edit/units-edit').then(c => c.UnitsEdit) },
  { path: 'edit', loadComponent: () => import('./pages/units-edit/units-edit').then(c => c.UnitsEdit) }
];
