import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  ɵInternalFormsSharedModule,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { NgClass, AsyncPipe } from '@angular/common';
import { Observable, tap, map, Subject, takeUntil } from 'rxjs';
import { Auth } from '../../auth';
import { NgIf } from '@angular/common';
import { AppConsole } from '../../../../utils/app-console';
import { httpResource } from '@angular/common/http';
import { RoleService } from '../../../../core/roles/role-service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ɵInternalFormsSharedModule,
    ReactiveFormsModule,
    RouterModule,
    NgClass,
    NgIf,
    AsyncPipe,
  ],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit, OnDestroy {
  form: FormGroup;
  loginResponse$?: Observable<any>;
  returnUrl: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: Auth,
    private route: ActivatedRoute,
    private router: Router,
    private roleService: RoleService
  ) {
    this.form = this.fb.group({
      emailUsername: ['', [Validators.required]],
      password: ['', [Validators.required]],
    });
  }

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'];
    AppConsole.log(`return url is: ${this.returnUrl}`);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit() {
    if (this.form.valid) {
      const loginRequest = {
        Username: this.form.value.emailUsername,
        PlainPassword: this.form.value.password,
      };
      this.loginResponse$ = this.authService.login(loginRequest).pipe(
        tap((res) => {
          if (!res.error) {
            // TODO to handle for non user login
            this.roleService
              .getRole()
              .pipe(takeUntil(this.destroy$))
              .subscribe((_) => {
                const url = this.returnUrl || '/dashboard/user';
                this.router.navigateByUrl(url);
              });
          }
        }),
        map((res) => (res.error ? res : null))
      );
    }
  }
}
