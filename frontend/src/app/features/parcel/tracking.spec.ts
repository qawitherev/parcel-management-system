import { TestBed } from '@angular/core/testing';

import { Tracking } from './tracking';

describe('Tracking', () => {
  let service: Tracking;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Tracking);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
