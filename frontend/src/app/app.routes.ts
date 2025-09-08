import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: '', loadChildren: () => import('./features/auth/auth-module').then(m => m.AuthModule)},
    { path: 'dashboard', loadChildren: () => import('./features/dashboard/dashboard-module')
            .then(m => m.DashboardModule)
    },
    {path: 'unauthorized', loadChildren: () => import('./system-pages/system-pages-module')
        .then(m => m.SystemPagesModule)
    },
    { path: 'parcels/tracking', loadChildren: () => import('./features/parcels/tracking/tracking-module')
        .then(m => m.TrackingModule)
    },
    { path: 'parcels/checkIn', loadChildren: () => import('./features/parcels/check-in/check-in-module')
        .then(m => m.CheckInModule)
    }

];
