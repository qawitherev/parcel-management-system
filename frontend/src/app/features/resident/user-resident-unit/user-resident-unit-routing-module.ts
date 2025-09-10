import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AddUserToUnit } from './page/add-user-to-unit/add-user-to-unit';
import { isAdminAndManagerAuthed, isLoggedInGuard } from '../../../core/guards/auth-guard-guard';

const routes: Routes = [
  { path: '', component: AddUserToUnit, canActivate: [isLoggedInGuard, isAdminAndManagerAuthed]}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserResidentUnitRoutingModule { }
