import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);
  // TODO: add toastr

  if (accountService.currentUser()){
    return true;
  } else{
    toastr.error('You have to be authenticated to proceed!');
    return false;
  }
};
