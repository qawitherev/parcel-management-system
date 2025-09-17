import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UnitsEdit } from './units-edit';

describe('UnitsEdit', () => {
  let component: UnitsEdit;
  let fixture: ComponentFixture<UnitsEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UnitsEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UnitsEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
