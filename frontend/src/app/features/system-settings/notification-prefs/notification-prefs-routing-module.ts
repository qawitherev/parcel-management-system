import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NotificationPrefs } from './notification-prefs';

const routes: Routes = [
  {
    path: '' ,component: NotificationPrefs
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class NotificationPrefsRoutingModule { }
