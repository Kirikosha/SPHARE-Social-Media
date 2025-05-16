import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { AccountModel } from '../_models/accountModel';
import { LoginModel } from '../_models/loginModel';
import { map } from 'rxjs';
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

  register(model: RegisterModel) {
    return this.http.post<AccountModel>(this.baseUrl + '/account/register', model).pipe(
      map(user => {
        if (user){
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);
        }
      })
    )
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUser.set(null);
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
