import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParcelsList } from './parcels-list';

describe('ParcelsList', () => {
  let component: ParcelsList;
  let fixture: ComponentFixture<ParcelsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ParcelsList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ParcelsList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
