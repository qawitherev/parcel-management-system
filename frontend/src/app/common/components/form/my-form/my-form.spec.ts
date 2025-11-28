import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyForm } from './my-form';

describe('MyForm', () => {
  let component: MyForm;
  let fixture: ComponentFixture<MyForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
