import { Routes } from '@angular/router';
import { Listing } from './pages/listing/listing';

export const LOCKER_ROUTES: Routes = [
  { path: '', component: Listing },
  { path: 'addEdit/:id', loadComponent: () => import('./pages/add-edit/add-edit').then(c => c.AddEdit) },
  { path: 'addEdit', loadComponent: () => import('./pages/add-edit/add-edit').then(c => c.AddEdit) }
];
