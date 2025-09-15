import { TestBed } from '@angular/core/testing';

import { LayoutServiceTs } from './layout-service.ts';

describe('LayoutServiceTs', () => {
  let service: LayoutServiceTs;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LayoutServiceTs);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
