import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchResult } from './search-result';

describe('SearchResult', () => {
  let component: SearchResult;
  let fixture: ComponentFixture<SearchResult>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SearchResult]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SearchResult);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
