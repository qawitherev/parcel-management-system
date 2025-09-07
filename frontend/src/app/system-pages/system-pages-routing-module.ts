import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Unauthorize } from './unauthorize/unauthorize';

const routes: Routes = [
  {path: '', component:Unauthorize}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SystemPagesRoutingModule { }
