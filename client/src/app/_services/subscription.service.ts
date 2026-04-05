import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { MemberModel } from '../_models/user/memberModel';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getSubscriptionCount(uniqueNameIdentifier: string) {
    return this.http.get<number>(this.baseUrl + '/subscription/subscriptions-count', { params: { uniqueNameIdentifier } });
  }

  getFollowerCount(uniqueNameIdentifier: string) {
    return this.http.get<number>(this.baseUrl + '/subscription/followers-count', { params: { uniqueNameIdentifier } });
  }

  getSubscriptions(uniqueNameIdentifier: string) {
    return this.http.get<MemberModel[]>(this.baseUrl + '/subscription/subscriptions', { params: { uniqueNameIdentifier } });
  }

  getFollowers(uniqueNameIdentifier: string) {
    return this.http.get<MemberModel[]>(this.baseUrl + '/subscription/followers', { params: { uniqueNameIdentifier } });
  }

  subscribe(uniqueNameIdentifier: string){
    return this.http.post(this.baseUrl + '/subscription/subscribe', 
  JSON.stringify(uniqueNameIdentifier), {
    headers: { 'Content-Type': 'application/json' }
  });
 
  }

  unsubscribe(uniqueNameIdentifier: string){
    return this.http.post(this.baseUrl + '/subscription/unsubscribe',
      JSON.stringify(uniqueNameIdentifier), {
        headers: { 'Content-Type': 'application/json' }
      });
  }

  isFollowing(uniqueNameIdentifier: string) {
    return this.http.get<boolean>(this.baseUrl + '/subscription/is-following', { params: { uniqueNameIdentifier } });
  }
  constructor() { }
}
