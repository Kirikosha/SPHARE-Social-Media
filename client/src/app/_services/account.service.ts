import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, Observable, tap, throwError} from 'rxjs';
import { environment } from '../../environments/environment';
import { AccountModel } from '../_models/accountModel';
import { LoginModel } from '../_models/loginModel';
import { isTokenExpired } from '../_utilities/jwtDecode';
import { TokenStorageService } from './token-storage.service';


@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private tokenStorage = inject(TokenStorageService);
  currentUser = this.tokenStorage.currentUser;
  isInitialized = signal<boolean>(false);
  baseUrl = environment.apiUrl;

  constructor() {
    const user = this.tokenStorage.getUser();
    if (user) {
      if (!isTokenExpired(user.token)) {
        this.tokenStorage.setUser(user);
        this.isInitialized.set(true);
      } else if (user.refreshToken) {
        this.refresh(user.refreshToken).subscribe({
          next: () => this.isInitialized.set(true),
          error: () => {
            this.logout();
            this.isInitialized.set(true);
          }
        });
      } else {
        this.logout();
        this.isInitialized.set(true);
      }
    } else {
      this.isInitialized.set(true);
    }
  }

  login(model: LoginModel): Observable<AccountModel> {
    return this.http.post<AccountModel>(this.baseUrl + '/account/login', model).pipe(
      tap(user => {
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
      tap(user => this.tokenStorage.setUser(user)),
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
    this.tokenStorage.clearUser();
    this.router.navigateByUrl('/login');
  }

  // keeps localStorage and signal in sync
  private setUser(user: AccountModel) {
    this.tokenStorage.setUser(user);
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
