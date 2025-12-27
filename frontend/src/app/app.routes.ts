import { Routes } from '@angular/router';
import { EmptyLayout } from './common/layout/empty-layout/empty-layout';
import { NormalLayout } from './common/layout/normal-layout/normal-layout';
import { isAdminAndManagerAuthed, isLoggedInGuard, isManagerAuthed } from './core/guards/auth-guard-guard';

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
        path: 'systemPages', 
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
                data: { title: 'Check In'}
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
        canActivate: [isLoggedInGuard]
    },

    {
        path: 'resident',
        component: NormalLayout, 
        children: [
            {
                path: 'userResidentUnit', loadChildren: () => import('./features/resident/user-resident-unit/user-resident-unit-module').then(m => m.UserResidentUnitModule), 
                data: { title: 'User Resident Unit'}
            }
        ],
        canActivate: [isLoggedInGuard]
    }, 

    {
        path: 'residentUnit', 
        component: NormalLayout, 
        children: [
            {
                path: 'units', loadChildren: () => import('./features/resident-units/units/units-module').then(m => m.UnitsModule),
                data: { title: 'Resident Units'}
            }
        ], 
        canActivate: [isLoggedInGuard]
    }, 

    {
        path: 'locker', 
        component: NormalLayout, 
        children: [
            {
                path: '', loadChildren: () => import('./features/locker/locker-module').then(m => m.LockerModule),
                data: { title: 'Locker'}
            }
        ], 
        canActivate: [isLoggedInGuard, isAdminAndManagerAuthed]
    }, 

    {
        path: 'settings', 
        component: NormalLayout, 
        children: [
            {
                path: 'notifications', loadChildren: () => import('./features/system-settings/notification-prefs/notification-prefs-module').then(m => m.NotificationPrefsModule),
                data: { title: 'Notifications'}, 
                canActivate: [isLoggedInGuard]
            }
        ]
    }
];
