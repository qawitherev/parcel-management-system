import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Units } from './pages/units/units';

const routes: Routes = [
  { path: '', component: Units}, 
  { path: 'edit/:id', loadComponent: () => import('./pages/units-edit/units-edit').then(c => c.UnitsEdit)},
  { path: 'edit', loadComponent: () => import('./pages/units-edit/units-edit').then(c => c.UnitsEdit)}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UnitsRoutingModule { }
