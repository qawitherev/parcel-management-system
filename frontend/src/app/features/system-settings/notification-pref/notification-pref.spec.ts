import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotificationPref } from './notification-pref';

describe('NotificationPref', () => {
  let component: NotificationPref;
  let fixture: ComponentFixture<NotificationPref>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationPref]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotificationPref);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
