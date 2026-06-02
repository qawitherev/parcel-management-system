import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { isLoggedInGuard, isAdminAuthed } from '../../core/guards/auth-guard-guard';

export const AUTH_ROUTES: Routes = [
  { path: 'login', component: Login },
  { path: 'register', loadComponent: () => import('./pages/register/register').then(c => c.Register) },
  {
    path: 'registerManager',
    loadComponent: () => import('./pages/register-manager/register-manager').then(m => m.RegisterManager),
    canActivate: [isLoggedInGuard, isAdminAuthed]
  }
];
