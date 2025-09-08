import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserAwaitingPickup } from './user-awaiting-pickup';

describe('UserAwaitingPickup', () => {
  let component: UserAwaitingPickup;
  let fixture: ComponentFixture<UserAwaitingPickup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserAwaitingPickup]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserAwaitingPickup);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
