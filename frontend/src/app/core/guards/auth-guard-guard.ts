import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { GuardsService } from './guards-service';
import { AppConsole } from '../../utils/app-console';
import { map } from 'rxjs';

export const isLoggedInGuard: CanActivateFn = (route, state) => {
  AppConsole.log(`isLoggedIn guard accessed`)
  const guardService = inject(GuardsService)
  const router = inject(Router)
  if (guardService.isAccessTokenExist()) {
    return true
  } 
  return false;
};

export const isAdminAuthed: CanActivateFn = (route, state) => {
  AppConsole.log(`isAdminAuthed accessed`)
  const guardService = inject(GuardsService)
  const router = inject(Router)
  return guardService.isRoleAuthorized$([`Admin`]).pipe(
    map(isAdmin => {
      if (isAdmin) return true
      return router.createUrlTree([`/unauthorized`])
    })
  )
}

export const isManagerAuthed: CanActivateFn = (route, state) => {
  AppConsole.log(`isManagerAuthed accessed`)
  const guardService = inject(GuardsService)
  const router = inject(Router)
  return guardService.isRoleAuthorized$([`ParcelRoomManager`]).pipe(
    map(isAdmin => {
      if (isAdmin) return true
      return router.createUrlTree([`/unauthorized`])
    })
  )
}

export const isResidentAuthed: CanActivateFn = (route, state) => {
  AppConsole.log(`isResidentAuthed accessed`)
  const guardService = inject(GuardsService)
  const router = inject(Router)
  return guardService.isRoleAuthorized$([`Resident`]).pipe(
    map(isAdmin => {
      if (isAdmin) return true
      return router.createUrlTree([`/unauthorized`])
    })
  )
}

export const isAdminAndManagerAuthed: CanActivateFn = (route, state) => {
  AppConsole.log(`isAdminAndManagerAuthed accessed`)
  const guardService = inject(GuardsService)
  const router = inject(Router)

  return guardService.isRoleAuthorized$([`Admin`, `ParcelRoomManager`]).pipe(
    map(isAdminOrManager => {
      if (isAdminOrManager) return true
      return router.createUrlTree(['/unauthorized'])
    })
  )
}
