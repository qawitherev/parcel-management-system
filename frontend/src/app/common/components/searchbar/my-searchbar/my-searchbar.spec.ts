import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MySearchbar } from './my-searchbar';

describe('MySearchbar', () => {
  let component: MySearchbar;
  let fixture: ComponentFixture<MySearchbar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MySearchbar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MySearchbar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
