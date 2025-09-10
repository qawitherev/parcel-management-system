import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddUserToUnit } from './add-user-to-unit';

describe('AddUserToUnit', () => {
  let component: AddUserToUnit;
  let fixture: ComponentFixture<AddUserToUnit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddUserToUnit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddUserToUnit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
