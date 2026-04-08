import { HttpClient, HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from 'rxjs';
import { AccountModel } from '../_models/accountModel';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { TokenStorageService } from '../_services/token-storage.service';

let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<AccountModel | null>(null);

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorageService);

  const isSignalRRequest =
    req.url.includes('/chat?') ||
    req.url.endsWith('/chat') ||
    req.url.includes('/chat/negotiate');

  const isRefreshRequest = req.url.includes('/account/refresh');

  const user = tokenStorage.currentUser();
  if (user?.token && !isSignalRRequest && !isRefreshRequest) {
    req = addToken(req, user.token);
  }

  return next(req).pipe(
    catchError(err => {
      if (err instanceof HttpErrorResponse && err.status === 401 && !isRefreshRequest) {
        return handle401(req, next, tokenStorage);
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
  tokenStorage: TokenStorageService
): Observable<any> {
  const user = tokenStorage.currentUser();
  const router = inject(Router);
  const http = inject(HttpClient);

  if (!user?.refreshToken) {
    tokenStorage.clearUser();
    router.navigateByUrl('/login');
    return throwError(() => new Error('No refresh token available'));
  }

  if (!isRefreshing) {
    isRefreshing = true;
    refreshDone$.next(null);

    return http.post<AccountModel>(
      environment.apiUrl + '/account/refresh',
      JSON.stringify(user.refreshToken),
      { headers: { 'Content-Type': 'application/json' } }
    ).pipe(
      switchMap(newUser => {
        isRefreshing = false;
        tokenStorage.setUser(newUser);
        refreshDone$.next(newUser);
        return next(addToken(req, newUser.token));
      }),
      catchError(err => {
        isRefreshing = false;
        tokenStorage.clearUser();
        router.navigateByUrl('/login');
        return throwError(() => err);
      })
    );
  }

  return refreshDone$.pipe(
    filter(u => u !== null),
    take(1),
    switchMap(u => next(addToken(req, u!.token)))
  );
}