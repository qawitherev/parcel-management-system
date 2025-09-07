import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserDashboardParent } from './user-dashboard-parent';

describe('UserDashboardParent', () => {
  let component: UserDashboardParent;
  let fixture: ComponentFixture<UserDashboardParent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserDashboardParent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserDashboardParent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
