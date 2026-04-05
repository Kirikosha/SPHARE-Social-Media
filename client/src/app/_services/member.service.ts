import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { MemberModel } from '../_models/user/memberModel';
import { environment } from '../../environments/environment';
import { UpdateMemberModel } from '../_models/user/updateMemberModel';

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

updateMember(model: UpdateMemberModel){
  const formData = new FormData();
  formData.append('Id', model.id.toString());
  formData.append('Username', model.username);
  formData.append('UniqueNameIdentifier', model.uniqueNameIdentifier);
  formData.append('JoinedAt', model.joinedAt);
  
  if (model.profileImage) {
    formData.append('ProfileImage', model.profileImage);
  }

  if (model.userProfileDetails) {
    const pascalCaseDetails = toPascalCase(model.userProfileDetails);
    formData.append('UserProfileDetails', JSON.stringify(pascalCaseDetails));
  }

  if (model.address) {
    const pascalCaseAddress = toPascalCase(model.address);
    console.log('Address to send:', pascalCaseAddress);
    console.log('Address JSON:', JSON.stringify(pascalCaseAddress));
    formData.append('Address', JSON.stringify(pascalCaseAddress));
  }

  let action = 'Keep'
  formData.append('Action', action);


  return this.http.put<MemberModel>(this.baseUrl + '/publicUser/edit', formData);
  }

  constructor() { }
}

function toPascalCase(obj: any): any {
  if (Array.isArray(obj)) {
    return obj.map(item => toPascalCase(item));
  }
  
  if (obj !== null && typeof obj === 'object') {
    const newObj: any = {};
    for (const key in obj) {
      if (obj.hasOwnProperty(key)) {
        // Convert camelCase to PascalCase
        const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
        newObj[pascalKey] = toPascalCase(obj[key]);
      }
    }
    return newObj;
  }
  
  return obj;
}

