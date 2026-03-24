import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from 'rxjs';
import { AccountModel } from '../_models/accountModel';

let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<AccountModel | null>(null);

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);

  const isSignalRRequest =
    req.url.includes('/chat?') ||
    req.url.endsWith('/chat') ||
    req.url.includes('/chat/negotiate');

  const user = accountService.currentUser();
  if (user?.token && !isSignalRRequest) {
    req = addToken(req, user.token);
  }

  return next(req).pipe(
    catchError(err => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        return handle401(req, next, accountService);
      }
      return throwError(() => err);
    })
  );
};

function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });
}

function handle401(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  accountService: AccountService
): Observable<any> {
  const user = accountService.currentUser();

  if (!user?.refreshToken) {
    accountService.logout();
    return throwError(() => new Error('No refresh token available'));
  }

  if (!isRefreshing) {
    // First request to hit 401 — start the refresh
    isRefreshing = true;
    refreshDone$.next(null);

    return accountService.refresh(user.refreshToken).pipe(
      switchMap(newUser => {
        isRefreshing = false;
        refreshDone$.next(newUser);
        // Retry the original request with the new token
        return next(addToken(req, newUser.token));
      }),
      catchError(err => {
        isRefreshing = false;
        accountService.logout();
        return throwError(() => err);
      })
    );
  }

  // Other requests that hit 401 while refresh is in progress — wait for it
  return refreshDone$.pipe(
    filter(user => user !== null),
    take(1),
    switchMap(user => next(addToken(req, user!.token)))
  );
}
