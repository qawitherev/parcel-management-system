import { Component, OnInit, signal } from '@angular/core';
import { AsyncPipe, NgIf } from '@angular/common';
import { catchError, Observable, of } from 'rxjs';
import { DashboardService, UserResponse } from '../../dashboard-service';
import { SkeletonComponent } from '../../../../common/components/skeleton/skeleton';
import { EmptyStateComponent } from '../../../../common/components/empty-state/empty-state';
import { ErrorCardComponent } from '../../../../common/components/error-card/error-card';

interface Stats {
  awaitingPickup: number;
  recentlyPickedUp: number;
  overstay: number;
  totalToday: number;
}

interface ParcelRow {
  trackingNumber: string;
  residentUnit: string;
  locker: string;
  status: string;
  waitingTime: string;
}

@Component({
  selector: 'app-dashboard-parent',
  standalone: true,
  imports: [NgIf, AsyncPipe, SkeletonComponent, EmptyStateComponent, ErrorCardComponent],
  templateUrl: './dashboard-parent.html',
  styleUrls: ['./dashboard-parent.css']
})
export class DashboardParent implements OnInit {
  loading = signal(true);
  error = signal<string | null>(null);
  stats = signal<Stats>({ awaitingPickup: 0, recentlyPickedUp: 0, overstay: 0, totalToday: 0 });
  awaitingParcels = signal<ParcelRow[]>([]);
  recentParcels = signal<ParcelRow[]>([]);
  user = signal<UserResponse | null>(null);

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.error.set(null);

    this.dashboardService.getAwaitingPickup().pipe(
      catchError(() => of(null))
    ).subscribe(data => {
      if (data && !data.error) {
        this.stats.update(s => ({ ...s, awaitingPickup: data.count || data.Parcels?.length || 0 }));
        if (data.Parcels) {
          this.awaitingParcels.set(data.Parcels.slice(0, 10));
        }
      } else {
        this.error.set('Failed to load awaiting parcels.');
      }
    });

    this.dashboardService.getRecentlyPickedUp().pipe(
      catchError(() => of(null))
    ).subscribe(data => {
      if (data && !data.error) {
        this.stats.update(s => ({ ...s, recentlyPickedUp: data.count || data.Parcels?.length || 0 }));
        if (data.Parcels) {
          this.recentParcels.set(data.Parcels.slice(0, 10));
        }
      }
    });

    this.dashboardService.getUserDetails().pipe(
      catchError(() => of(null))
    ).subscribe(data => {
      if (data && !('error' in data)) {
        this.user.set(data as UserResponse);
      }
      this.loading.set(false);
    });
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
      case 'overstay': return 'Overstay';
      default: return status || 'Unknown';
    }
  }

  get userName(): string {
    return this.user()?.Username?.split(' ')[0] || 'Manager';
  }
}
