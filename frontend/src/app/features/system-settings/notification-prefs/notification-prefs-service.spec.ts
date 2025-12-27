import { TestBed } from '@angular/core/testing';

import { NotificationPrefsService } from './notification-prefs-service';

describe('NotificationPrefsService', () => {
  let service: NotificationPrefsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationPrefsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
