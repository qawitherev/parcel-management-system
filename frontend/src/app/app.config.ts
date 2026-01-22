import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptors } from '@angular/common/http';
import { AttachTokenInterceptor } from './core/interceptors/auth-interceptor';
import { ErrorResponseInterceptor } from './core/error-handling/error-response-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors(
        [AttachTokenInterceptor, ErrorResponseInterceptor]
      )
    ),
    // { provide: HTTP_INTERCEPTORS, useClass: AttachTokenInterceptor, multi: true}
  ]
};
