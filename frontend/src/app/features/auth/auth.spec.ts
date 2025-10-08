import { TestBed } from '@angular/core/testing';
import { Auth } from './auth';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RoleService } from '../../core/roles/role-service';
import { Router } from '@angular/router';

describe('Auth', () => {
  let service: Auth;
  let httpMock: any;

  beforeEach(() => {
    const roleSpy = jasmine.createSpyObj('RoleService', ['clearRole'])
    const routerSpy = jasmine.createSpyObj('Router', ['navigateByUrl'])
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(), 
        provideHttpClientTesting(),
        { provide: RoleService, useValue: roleSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });
    service = TestBed.inject(Auth);
    httpMock = TestBed.inject(HttpTestingController)
  });

  afterEach(() => {
    httpMock.verify()
  })

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
