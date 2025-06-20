import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { AccountModel } from '../_models/accountModel';
import { LoginModel } from '../_models/loginModel';
import { map, Observable } from 'rxjs';
import { RegisterModel } from '../_models/registerModel';
import { JsonPipe } from '@angular/common';
import { isTokenExpired } from '../_utilities/jwtDecode';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private router = inject(Router);
  baseUrl = environment.apiUrl;
  currentUser = signal<AccountModel | null>(null);

  login (model: LoginModel){
    return this.http.post<AccountModel>(this.baseUrl + '/account/login', model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);
        }
      })
    )
  }
register(model: FormData) {
  return this.http.post<AccountModel>(this.baseUrl + '/account/register', model).pipe(
    map(user => {
      if (user) {
        localStorage.setItem('user', JSON.stringify(user));
        this.currentUser.set(user);
      }
      return user;
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

  logout(){
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

  forgotPassword(email: string){
    return this.http.post(this.baseUrl + '/account/forgot-password', { email });
  }

  verifyResetCode(email: string, code: string){
    return this.http.post(`${this.baseUrl}/account/verify-reset-code`, { email, code });
  }

  resetPassword(email: string, newPassword: string){
    return this.http.post(this.baseUrl + '/account/reset-password', { email, newPassword });
  }

  constructor(){
    const userJson = localStorage.getItem('user');
    if (userJson) {
      const user = JSON.parse(userJson);
      if (isTokenExpired(user.token)){
        this.logout();
      } else {
        this.currentUser.set(user);
      }
    }
  }
}
