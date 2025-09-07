import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Unauthorize } from './unauthorize';

describe('Unauthorize', () => {
  let component: Unauthorize;
  let fixture: ComponentFixture<Unauthorize>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Unauthorize]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Unauthorize);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
