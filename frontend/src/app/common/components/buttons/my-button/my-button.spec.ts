import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyButton } from './my-button';

describe('MyButton', () => {
  let component: MyButton;
  let fixture: ComponentFixture<MyButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyButton]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyButton);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
