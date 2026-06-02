import { Routes } from '@angular/router';
import { AddUserToUnit } from './page/add-user-to-unit/add-user-to-unit';
import { isLoggedInGuard, isAdminAndManagerAuthed } from '../../../core/guards/auth-guard-guard';

export const USER_RESIDENT_UNIT_ROUTES: Routes = [
  { path: '', component: AddUserToUnit, canActivate: [isLoggedInGuard, isAdminAndManagerAuthed] }
];
