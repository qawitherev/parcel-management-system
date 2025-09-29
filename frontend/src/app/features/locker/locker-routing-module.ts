import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Listing } from './pages/listing/listing';

const routes: Routes = [
  { path: '', component: Listing},
  { path: 'addEdit/:id', loadComponent: () => import('./pages/add-edit/add-edit').then(c => c.AddEdit)}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LockerRoutingModule { }
