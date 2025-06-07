import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { MemberModel } from '../_models/memberModel';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  
  getMemberByUniqueNameIdentifier(uNI: string) {
    return this.http.get<MemberModel>(this.baseUrl + '/member/by-uni', { params: { uNI } })
  }

  getMyProfile(){
    return this.http.get<MemberModel>(this.baseUrl + '/member/my-profile');
  }

  constructor() { }
}
