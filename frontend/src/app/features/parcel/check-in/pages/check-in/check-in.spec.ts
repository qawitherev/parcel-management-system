import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckIn } from './check-in';

describe('CheckIn', () => {
  let component: CheckIn;
  let fixture: ComponentFixture<CheckIn>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CheckIn]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CheckIn);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
