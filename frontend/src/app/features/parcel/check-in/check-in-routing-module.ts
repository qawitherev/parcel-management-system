import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CheckIn } from './pages/check-in/check-in';
import { isAdminAndManagerAuthed, isLoggedInGuard } from '../../../core/guards/auth-guard-guard';

const routes: Routes = [
  { path: '', component: CheckIn, canActivate: [isLoggedInGuard, isAdminAndManagerAuthed]}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CheckInRoutingModule { }
