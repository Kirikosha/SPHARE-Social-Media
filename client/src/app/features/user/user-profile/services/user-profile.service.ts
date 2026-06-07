import {inject, Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {environment} from "../../../../../environments/environment";
import {UserDto} from "../models/user.dto";

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  getProfile(uNI: string) {
    return this.http.get<UserDto>(this.baseUrl + '/publicUser/by-uni', {params: {uNI}});
  }
  constructor() { }
}
