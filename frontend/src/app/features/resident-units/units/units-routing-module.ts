import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Units } from './pages/units/units';

const routes: Routes = [
  { path: '', component: Units}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UnitsRoutingModule { }
