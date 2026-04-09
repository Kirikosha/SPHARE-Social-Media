import { Injectable, signal } from '@angular/core';
import { AccountModel } from '../_models/accountModel';

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  currentUser = signal<AccountModel | null>(null);

  getUser(): AccountModel | null {
    const json = localStorage.getItem('user');
    return json ? JSON.parse(json) : null;
  }

  setUser(user: AccountModel) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  clearUser() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }
}
