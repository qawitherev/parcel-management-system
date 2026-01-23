import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth-service';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RoleService } from '../../core/roles/role-service';
import { Router } from '@angular/router';
import { AuthEndpoints } from '../../core/endpoints/auth-endpoints';
import { provideZonelessChangeDetection } from '@angular/core';

fdescribe('Auth', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    const roleSpy = jasmine.createSpyObj('RoleService', ['clearRole']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigateByUrl']);
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: RoleService, useValue: roleSpy },
        { provide: Router, useValue: routerSpy },
      ],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // make sure no request is left hanging
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('register()', () => {
    it('should send register with correct payload and receive correct response', () => {
      const registerPayload = {
        Username: 'testUser',
        Email: 'email@email',
        ResidentUnit: 'RU001',
        Password: '123',
      };

      const expectedResponse = {
        UserId: '001',
        Username: 'testUser',
      };

      service.register(registerPayload).subscribe((res) => {
        expect(res).toEqual(expectedResponse);
      });

      // catches the request made by service.register()
      const req = httpMock.expectOne(AuthEndpoints.register);
      expect(req.request.body).toEqual(registerPayload);
      req.flush(expectedResponse);
    });
  });
});
