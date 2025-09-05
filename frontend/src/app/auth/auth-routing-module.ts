import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login as LoginComponent } from './login/login';
import { isLoggedInGuard } from '../core/guards/auth-guard-guard';

const routes: Routes = [
  {path: 'login', component: LoginComponent}, 
  {path: 'register', loadComponent: () => import('./register/register').then(c => c.Register)}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
