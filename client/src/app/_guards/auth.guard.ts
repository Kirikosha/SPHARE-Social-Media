import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { toObservable } from '@angular/core/rxjs-interop';
import { filter, take, map } from 'rxjs';

export const authGuard: CanActivateFn = () => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  return toObservable(accountService.isInitialized).pipe(
    filter(initialized => initialized),  // wait until true
    take(1),
    map(() => {
      if (accountService.currentUser()) {
        return true;
      }
      router.navigateByUrl('/login');
      return false;
    })
  );
};
