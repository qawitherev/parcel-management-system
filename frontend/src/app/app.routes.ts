import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: '', loadChildren: () => import('./auth/auth-module').then(m => m.AuthModule)},
    { path: 'dashboard', loadChildren: () => import('./dashboard/dashboard-module')
            .then(m => m.DashboardModule)
    }
];
