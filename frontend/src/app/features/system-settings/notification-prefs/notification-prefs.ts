import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import {
  NotificationPrefResponse,
  NotificationPrefsService,
  NotificationPrefUpdateRequest,
} from './notification-prefs-service';
import { Observable, tap } from 'rxjs';
import { ApiError } from '../../../core/error-handling/api-catch-error';
import { AsyncPipe } from '@angular/common';
import { MySwitch } from '../../../common/components/switch/my-switch/my-switch';
import { MyButton } from '../../../common/components/buttons/my-button/my-button';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-notification-prefs',
  imports: [AsyncPipe, MySwitch, MyButton, FormsModule],
  templateUrl: './notification-prefs.html',
  styleUrl: './notification-prefs.css',
})
export class NotificationPrefs implements OnInit {
  notificationPref$?: Observable<NotificationPrefResponse | ApiError>;
  isQuietHoursEnabled?: boolean;
  isLoading: boolean = false;
  prefState?: NotificationPrefResponse;

  constructor(private npService: NotificationPrefsService) {}

  ngOnInit(): void {
    this.notificationPref$ = this.npService.getNotificationPrefByUser().pipe(
      tap((res) => {
        if (res && 'id' in res) {
          this.prefState = res;
          if (res.quietHoursFrom === null || res.quietHoursTo === null) {
            this.isQuietHoursEnabled = false;
          }
        }
      })
    );
  }

  onToggleEmail(): void {
    if (this.prefState?.isEmailActive) {
      this.prefState.isEmailActive = !this.prefState.isEmailActive;
    }
  }

  onToggleWhatsApp(): void {
    if (this.prefState?.isWhatsAppActive !== undefined) {
      this.prefState.isWhatsAppActive = !this.prefState.isWhatsAppActive;
    }
  }

  onToggleOnClaim(): void {
    if (this.prefState?.isOnClaimActive !== undefined) {
      this.prefState.isOnClaimActive = !this.prefState.isOnClaimActive;
    }
  }

  onToggleOnCheckIn(): void {
    if (this.prefState?.isOnCheckInActive !== undefined) {
      this.prefState.isOnCheckInActive = !this.prefState.isOnCheckInActive;
    }
  }

  onToggleOverdue(): void {
    if (this.prefState?.isOverdueActive !== undefined) {
      this.prefState.isOverdueActive = !this.prefState.isOverdueActive;
    }
  }

  onToggleQuietHours(): void {
    this.isQuietHoursEnabled = !this.isQuietHoursEnabled;
  }

  get isCanApply(): boolean {
    if (!this.isQuietHoursEnabled) return true;
    else if (this.isQuietHoursEnabled && this.prefState?.quietHoursFrom != null && this.prefState?.quietHoursTo != null) return true;
    else return false;
  }

  onClickUpdateNotificationPref(): void {
    const payload: NotificationPrefUpdateRequest = {
      isEmailActive: this.prefState?.isEmailActive,
      isWhatsAppActive: this.prefState?.isWhatsAppActive,
      isOnCheckInActive: this.prefState?.isOnCheckInActive,
      isOnClaimActive: this.prefState?.isOnClaimActive,
      isOverdueActive: this.prefState?.isOverdueActive,
      quietHoursFrom: this.prefState?.quietHoursFrom,
      quietHoursTo: this.prefState?.quietHoursTo,
    };
    this.npService.updateNotificationPref(this.prefState?.id!, payload);
  }
}
