import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { GuardsService } from './guards-service';
import { AppConsole } from '../../utils/app-console';
import { map } from 'rxjs';

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



export const isAdminAndManagerAuthed: CanActivateFn = (route, state) => {
  AppConsole.log(`isAuthorized guard accessed`)
  const guardService = inject(GuardsService)
  const router = inject(Router)

  return guardService.isRoleAuthorized$([`Admin`, `ParcelRoomManager`]).pipe(
    map(isAdmin => {
      if (isAdmin) return true
      return router.createUrlTree(['/unauthorized'])
    })
  )
}
