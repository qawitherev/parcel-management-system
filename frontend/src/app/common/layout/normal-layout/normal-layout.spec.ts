import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NormalLayout } from './normal-layout';

describe('NormalLayout', () => {
  let component: NormalLayout;
  let fixture: ComponentFixture<NormalLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NormalLayout]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NormalLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
