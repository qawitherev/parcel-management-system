import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RecentlyPickedUp } from './recently-picked-up';

describe('RecentlyPickedUp', () => {
  let component: RecentlyPickedUp;
  let fixture: ComponentFixture<RecentlyPickedUp>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecentlyPickedUp]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RecentlyPickedUp);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
