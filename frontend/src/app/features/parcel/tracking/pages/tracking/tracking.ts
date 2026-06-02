import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { NgIf } from '@angular/common';
import { catchError, finalize } from 'rxjs';
import { TrackingService } from '../../../tracking';
import { SkeletonComponent } from '../../../../../common/components/skeleton/skeleton';
import { EmptyStateComponent } from '../../../../../common/components/empty-state/empty-state';
import { ErrorCardComponent } from '../../../../../common/components/error-card/error-card';

interface ParcelHistoryItem {
  eventTime: string;
  event: string;
  performedByUser: string;
}

interface TrackingResult {
  trackingNumber: string;
  entryDate: string;
  currentStatus: string;
  history: ParcelHistoryItem[];
}

@Component({
  selector: 'app-tracking',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, SkeletonComponent, EmptyStateComponent, ErrorCardComponent],
  templateUrl: './tracking.html',
  styleUrls: ['./tracking.css']
})
export class Tracking {
  formGroup: FormGroup;
  loading = signal(false);
  error = signal<string | null>(null);
  result = signal<TrackingResult | null>(null);
  searched = signal(false);

  constructor(private fb: FormBuilder, private trackingService: TrackingService) {
    this.formGroup = fb.group({
      searchKeyword: ['', [Validators.required]]
    });
  }

  onSubmit() {
    if (!this.formGroup.valid) return;
    this.loading.set(true);
    this.error.set(null);
    this.result.set(null);
    this.searched.set(true);

    this.trackingService.getUserParcelHistory(this.formGroup.value.searchKeyword).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (res) => {
        if (res.error) {
          this.error.set(res.message || 'Failed to find parcel');
        } else {
          this.result.set({
            trackingNumber: res.trackingNumber,
            entryDate: res.entryDate,
            currentStatus: res.currentStatus,
            history: res.history || []
          });
        }
      },
      error: () => this.error.set('Could not connect to server. Try again.')
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
      case 'awaitingpickup': return 'Awaiting Pickup';
      case 'pickedup': return 'Claimed';
      case 'claimed': return 'Claimed';
      case 'overstay': return 'Overstay';
      default: return status || 'Unknown';
    }
  }

  formatDate(iso: string): string {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  formatTime(iso: string): string {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' });
  }
}
