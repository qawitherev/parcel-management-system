import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardParent } from './dashboard-parent';

describe('DashboardParent', () => {
  let component: DashboardParent;
  let fixture: ComponentFixture<DashboardParent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardParent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DashboardParent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
