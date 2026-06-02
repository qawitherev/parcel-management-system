import { Routes } from '@angular/router';
import { CheckIn } from './pages/check-in/check-in';
import { isLoggedInGuard, isAdminAndManagerAuthed } from '../../../core/guards/auth-guard-guard';

export const CHECK_IN_ROUTES: Routes = [
  { path: '', component: CheckIn, canActivate: [isLoggedInGuard, isAdminAndManagerAuthed] }
];
