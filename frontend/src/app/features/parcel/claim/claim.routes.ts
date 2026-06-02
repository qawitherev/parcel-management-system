import { Routes } from '@angular/router';
import { Claim } from './pages/claim/claim';
import { isLoggedInGuard, isResidentAuthed } from '../../../core/guards/auth-guard-guard';

export const CLAIM_ROUTES: Routes = [
  { path: '', component: Claim, canActivate: [isLoggedInGuard, isResidentAuthed] }
];
