import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResultItem } from './result-item';

describe('ResultItem', () => {
  let component: ResultItem;
  let fixture: ComponentFixture<ResultItem>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResultItem]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ResultItem);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
