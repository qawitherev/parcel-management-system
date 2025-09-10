import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Claim } from './pages/claim/claim';
import { isLoggedInGuard, isManagerAuthed, isResidentAuthed } from '../../../core/guards/auth-guard-guard';

const routes: Routes = [
  { path: '', component: Claim, canActivate: [isLoggedInGuard, isResidentAuthed]}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ClaimRoutingModule { }
