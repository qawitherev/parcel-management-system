import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MySwitch } from './my-switch';

describe('MySwitch', () => {
  let component: MySwitch;
  let fixture: ComponentFixture<MySwitch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MySwitch]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MySwitch);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
