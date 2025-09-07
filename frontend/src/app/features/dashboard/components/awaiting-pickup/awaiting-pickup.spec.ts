import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AwaitingPickup } from './awaiting-pickup';

describe('AwaitingPickup', () => {
  let component: AwaitingPickup;
  let fixture: ComponentFixture<AwaitingPickup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AwaitingPickup]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AwaitingPickup);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
