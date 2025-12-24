import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotificationPrefs } from './notification-prefs';

describe('NotificationPrefs', () => {
  let component: NotificationPrefs;
  let fixture: ComponentFixture<NotificationPrefs>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationPrefs]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotificationPrefs);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
