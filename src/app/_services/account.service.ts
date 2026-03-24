import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { AccountModel } from '../_models/accountModel';
import { LoginModel } from '../_models/loginModel';
import { catchError, map, Observable, tap, throwError} from 'rxjs';
import { isTokenExpired } from '../_utilities/jwtDecode';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private router = inject(Router);
  baseUrl = environment.apiUrl;
  currentUser = signal<AccountModel | null>(null);

  constructor() {
    const userJson = localStorage.getItem('user');
    if (userJson) {
      const user: AccountModel = JSON.parse(userJson);

      if (!isTokenExpired(user.token)) {
        // Access token still valid — load normally
        this.currentUser.set(user);
      } else if (user.refreshToken) {
        // Access token expired but refresh token exists — try silent refresh
        this.refresh(user.refreshToken).subscribe({
          next: () => {},
          error: () => this.logout()
        });
      } else {
        this.logout();
      }
    }
  }

  login(model: LoginModel): Observable<void> {
    return this.http.post<AccountModel>(this.baseUrl + '/account/login', model).pipe(
      map(user => {
        if (user) {
          this.setUser(user);
        }
      })
    );
  }

  register(model: FormData): Observable<AccountModel> {
    return this.http.post<AccountModel>(this.baseUrl + '/account/register', model).pipe(
      map(user => {
        if (user) {
          this.setUser(user);
        }
        return user;
      })
    );
  }

  refresh(refreshToken: string): Observable<AccountModel> {
    return this.http.post<AccountModel>(
      this.baseUrl + '/account/refresh',
      JSON.stringify(refreshToken), 
      { headers: { 'Content-Type': 'application/json' } }
    ).pipe(
      tap(user => this.setUser(user)),
      catchError(err => {
        this.logout();
        return throwError(() => err);
      })
    );
  }

  updateProfile(model: AccountModel) {
    const user = this.currentUser();
    if (user) {
      user.uniqueNameIdentifier = model.uniqueNameIdentifier;
      user.username = model.username;
      this.currentUser.set(user);
      localStorage.setItem('user', JSON.stringify(user));
    }
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.router.navigateByUrl('/login');
  }

  // keeps localStorage and signal in sync
  private setUser(user: AccountModel) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  forgotPassword(email: string) {
    return this.http.post(this.baseUrl + '/account/forgot-password', { email });
  }

  verifyResetCode(email: string, code: string) {
    return this.http.post(`${this.baseUrl}/account/verify-reset-code`, { email, code });
  }

  resetPassword(email: string, newPassword: string) {
    return this.http.post(this.baseUrl + '/account/reset-password', { email, newPassword });
  }
}
