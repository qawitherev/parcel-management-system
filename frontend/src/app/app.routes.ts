import { Routes } from '@angular/router';
import { EmptyLayout } from './common/layout/empty-layout/empty-layout';
import { NormalLayout } from './common/layout/normal-layout/normal-layout';
import { isLoggedInGuard } from './core/guards/auth-guard-guard';

export const routes: Routes = [
    // { path: '', redirectTo: 'login', pathMatch: 'full' },
    // { path: '', loadChildren: () => import('./features/auth/auth-module').then(m => m.AuthModule)},
    // {path: 'unauthorized', loadChildren: () => import('./system-pages/system-pages-module')
    //     .then(m => m.SystemPagesModule)
    // },
    // { path: 'parcels/tracking', loadChildren: () => import('./features/parcel/tracking/tracking-module')
    //     .then(m => m.TrackingModule)
    // },
    // { path: 'parcels/checkIn', loadChildren: () => import('./features/parcel/check-in/check-in-module')
    //     .then(m => m.CheckInModule)
    // }, 
    // { path: 'resident/userResidentUnit', loadChildren: () => import('./features/resident/user-resident-unit/user-resident-unit-module')
    //     .then(m => m.UserResidentUnitModule)
    // }, 
    // { path: 'parcels/claim', loadChildren: () => import('./features/parcel/claim/claim-module')
    //     .then(m => m.ClaimModule)
    // }, 
    // { path: 'parcels/parcels', loadChildren: () => import('./features/parcel/parcels/parcels-module')
    //     .then(m => m.ParcelsModule)
    // },

    // LAYOUT BASED NAVIGATION
    {
        path: '', 
        component: EmptyLayout, 
        children: [
            {
                path: '', redirectTo: 'login', pathMatch: 'full'
            }, 
            {
                path: '', loadChildren: () => import('./features/auth/auth-module').then(m => m.AuthModule)
            }
        ]
    },

    {
        path: 'unauthorized', 
        component: EmptyLayout, 
        children: [
            {
                path: '', loadChildren: () => import('./system-pages/system-pages-module').then(m => m.SystemPagesModule)
            }
        ]
    },

    {
        path: 'parcel', 
        component: NormalLayout, 
        children: [
            {
                path: 'tracking', loadChildren: () => import('./features/parcel/tracking/tracking-module').then(m => m.TrackingModule)
            }, 
            {
                path: 'checkIn', loadChildren: () => import('./features/parcel/check-in/check-in-module').then(m => m.CheckInModule)
            }, 
            {
                path: 'claim', loadChildren: () => import('./features/parcel/claim/claim-module').then(m => m.ClaimModule)
            }, 
            {
                path: 'parcels', loadChildren: () => import('./features/parcel/parcels/parcels-module').then(m => m.ParcelsModule)
            }
        ], 
        canActivate: [isLoggedInGuard]
    }, 

    {
        path: 'dashboard', 
        component: NormalLayout,
        children: [
            {
                path: '', loadChildren: () => import('./features/dashboard/dashboard-module').then(m => m.DashboardModule)
            }
        ], 
    },

    {
        path: 'resident',
        component: NormalLayout, 
        children: [
            {
                path: 'userResidentUnit', loadChildren: () => import('./features/resident/user-resident-unit/user-resident-unit-module').then(m => m.UserResidentUnitModule)
            }
        ]
    }
];
