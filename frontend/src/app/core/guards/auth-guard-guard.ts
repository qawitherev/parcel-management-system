import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { GuardsService } from './guards-service';
import { AppConsole } from '../../utils/app-console';

export const isLoggedInGuard: CanActivateFn = (route, state) => {
  AppConsole.log(`isLoggedIn guard accessed`)
  const guardService = inject(GuardsService)
  const router = inject(Router)
  if (guardService.isLoggedIn()) {
    return true
  } 
  return router.createUrlTree(['/login'], 
    {
      queryParams : {returnUrl: state.url}
    }
  )
};
