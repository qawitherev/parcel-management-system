import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Claim } from './claim';

describe('Claim', () => {
  let component: Claim;
  let fixture: ComponentFixture<Claim>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Claim]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Claim);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
