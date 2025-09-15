import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login as LoginComponent } from './pages/login/login';
import { isLoggedInGuard, isAdminAndManagerAuthed, isAdminAuthed } from '../../core/guards/auth-guard-guard'

const routes: Routes = [
  {path: 'login', component: LoginComponent}, 
  {path: 'register', loadComponent: () => import('./pages/register/register').then(c => c.Register)},
  {path: 'registerManager', loadComponent: () => import('./pages/register-manager/register-manager')
      .then(m => m.RegisterManager), canActivate:[isLoggedInGuard, isAdminAuthed]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
