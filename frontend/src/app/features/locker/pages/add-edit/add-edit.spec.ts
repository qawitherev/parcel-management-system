import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEdit } from './add-edit';

describe('AddEdit', () => {
  let component: AddEdit;
  let fixture: ComponentFixture<AddEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
