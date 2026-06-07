import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.js';
import { MemberModel } from '../_models/user/memberModel.js';
import { UpdateMemberModel } from '../_models/user/updateMemberModel.js';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  getMemberByUniqueNameIdentifier(uNI: string) {
    return this.http.get<MemberModel>(this.baseUrl + '/publicUser/by-uni', { params: { uNI } })
  }

  getMyProfile(){
    return this.http.get<MemberModel>(this.baseUrl + '/publicUser/my-profile');
  }

  getOtherProfile(uniqueNameIdentifier: string){
    return this.http.get<MemberModel>(this.baseUrl + `/publicUser/other-user-profile`, { params: { uniqueNameIdentifier } });
  }

  searchForUser(searchQuery: string){
    return this.http.get<MemberModel[]>(this.baseUrl + '/publicUser/user-search', { params: { searchQuery } });
  }

  constructor() { }
}

