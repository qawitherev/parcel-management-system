import { Routes } from '@angular/router';
import { ParcelsList } from './pages/parcels-list/parcels-list';
import { isLoggedInGuard } from '../../../core/guards/auth-guard-guard';

export const PARCELS_ROUTES: Routes = [
  { path: '', component: ParcelsList, canActivate: [isLoggedInGuard] }
];
