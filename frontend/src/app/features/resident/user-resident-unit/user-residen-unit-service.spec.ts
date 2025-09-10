import { TestBed } from '@angular/core/testing';

import { UserResidenUnitService } from './user-residen-unit-service';

describe('UserResidenUnitService', () => {
  let service: UserResidenUnitService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserResidenUnitService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
