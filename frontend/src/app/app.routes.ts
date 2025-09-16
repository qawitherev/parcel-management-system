import { Routes } from '@angular/router';
import { EmptyLayout } from './common/layout/empty-layout/empty-layout';
import { NormalLayout } from './common/layout/normal-layout/normal-layout';
import { isLoggedInGuard } from './core/guards/auth-guard-guard';

export const routes: Routes = [

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
                path: 'tracking', loadChildren: () => import('./features/parcel/tracking/tracking-module').then(m => m.TrackingModule), 
                data: { title: 'Parcel Tracking'}
            }, 
            {
                path: 'checkIn', loadChildren: () => import('./features/parcel/check-in/check-in-module').then(m => m.CheckInModule),
                data: { title: 'Parcel Tracking'}
            }, 
            {
                path: 'claim', loadChildren: () => import('./features/parcel/claim/claim-module').then(m => m.ClaimModule),
                data: { title: 'Parcel Claim'}
            }, 
            {
                path: 'parcels', loadChildren: () => import('./features/parcel/parcels/parcels-module').then(m => m.ParcelsModule),
                data: { title: 'All Parcels'}
            }
        ], 
        canActivate: [isLoggedInGuard]
    }, 

    {
        path: 'dashboard', 
        component: NormalLayout,
        children: [
            {
                path: '', loadChildren: () => import('./features/dashboard/dashboard-module').then(m => m.DashboardModule), 
                data: {title: 'Dashboard'}
            }
        ], 
    },

    {
        path: 'resident',
        component: NormalLayout, 
        children: [
            {
                path: 'userResidentUnit', loadChildren: () => import('./features/resident/user-resident-unit/user-resident-unit-module').then(m => m.UserResidentUnitModule), 
                data: { title: 'User Resident Unit'}
            }
        ]
    }, 

    {
        path: 'residentUnit', 
        component: NormalLayout, 
        children: [
            {
                path: 'units', loadChildren: () => import('./features/resident-units/units/units-module').then(m => m.UnitsModule),
                data: { title: 'Resident Units'}
            }
        ]
    }
];
