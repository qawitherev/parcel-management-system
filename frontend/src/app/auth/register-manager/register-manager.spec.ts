import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterManager } from './register-manager';

describe('RegisterManager', () => {
  let component: RegisterManager;
  let fixture: ComponentFixture<RegisterManager>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterManager]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisterManager);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
