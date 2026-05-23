import { Component, OnInit, signal } from '@angular/core';
import { NgIf } from '@angular/common';
import { catchError, forkJoin, of } from 'rxjs';
import { DashboardService, UserResponse } from '../../dashboard-service';
import { SkeletonComponent } from '../../../../common/components/skeleton/skeleton';
import { EmptyStateComponent } from '../../../../common/components/empty-state/empty-state';
import { ErrorCardComponent } from '../../../../common/components/error-card/error-card';

interface ParcelRow {
  trackingNumber: string;
  locker: string;
  status: string;
  weight?: number;
  entryDate?: string;
}

@Component({
  selector: 'app-dashboard-user',
  standalone: true,
  imports: [NgIf, SkeletonComponent, EmptyStateComponent, ErrorCardComponent],
  templateUrl: './dashboard-user.html',
  styleUrls: ['./dashboard-user.css']
})
export class DashboardUser implements OnInit {
  loading = signal(true);
  error = signal<string | null>(null);
  user = signal<UserResponse | null>(null);
  awaitingCount = signal(0);
  claimedCount = signal(0);
  awaitingParcels = signal<ParcelRow[]>([]);
  claimedParcels = signal<ParcelRow[]>([]);

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.error.set(null);

    forkJoin([
      this.dashboardService.getUserDetails().pipe(catchError(() => of(null))),
      this.dashboardService.getUserAwaitingPickup().pipe(catchError(() => of(null))),
      this.dashboardService.getRecentlyPickedUp().pipe(catchError(() => of(null))),
    ]).subscribe({
      next: ([userRes, awaitingRes, claimedRes]) => {
        if (userRes && !('error' in userRes)) {
          this.user.set(userRes as UserResponse);
        }

        if (awaitingRes && !awaitingRes.error) {
          this.awaitingCount.set(awaitingRes.count || awaitingRes.Parcels?.length || 0);
          if (awaitingRes.Parcels) {
            this.awaitingParcels.set(awaitingRes.Parcels.slice(0, 10));
          }
        }

        if (claimedRes && !claimedRes.error) {
          this.claimedCount.set(claimedRes.count || claimedRes.Parcels?.length || 0);
          if (claimedRes.Parcels) {
            this.claimedParcels.set(claimedRes.Parcels.slice(0, 10));
          }
        }

        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load dashboard. Check your connection.');
        this.loading.set(false);
      }
    });
  }

  get userName(): string {
    return this.user()?.Username?.split(' ')[0] || 'Resident';
  }

  getStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'awaitingpickup': return 'b-await';
      case 'pickedup': case 'claimed': return 'b-claim';
      case 'overstay': return 'b-over';
      default: return '';
    }
  }

  getStatusLabel(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'awaitingpickup': return 'Awaiting';
      case 'pickedup': return 'Claimed';
      case 'claimed': return 'Claimed';
      default: return status || '—';
    }
  }
}
