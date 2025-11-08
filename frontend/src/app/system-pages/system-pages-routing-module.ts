import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Unauthorize } from './unauthorize/unauthorize';

const routes: Routes = [
  { path: '', component: Unauthorize },
  {
    path: 'uiComponent',
    loadComponent: () => import('./ui-component/ui-component').then((c) => c.UiComponent),
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SystemPagesRoutingModule {}
