import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../_services/account.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);

    const isSignalRRequest = 
    req.url.includes('/chat?') ||
    req.url.endsWith('/chat') ||
    req.url.includes('/chat/negotiate');
  if(accountService.currentUser()){
    const token = accountService.currentUser()?.token
    if (token && !isSignalRRequest) {
      const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${accountService.currentUser()?.token}`
      }})

      return next(cloned);
    }
  }
  return next(req);
};
