import { TestBed } from '@angular/core/testing';

import { TrackingService } from './tracking';

describe('Tracking', () => {
  let service: TrackingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TrackingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
