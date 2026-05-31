import {inject, Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {environment} from "../../../../../environments/environment";
import {UpdateAddressDto, UpdateMainInfoDto, UpdateProfileImageDto, UserUpdateDataDto} from "../models/update-user.dto";
import {Observable} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class UserEditService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  updateUserProfilePicture(model: UpdateProfileImageDto) {
    const formData = new FormData();
    formData.append('profileImage', model.profileImage); // key must match DTO property name

    return this.http.put(this.baseUrl + "/PublicUser/update-profile-image", formData);
  }

  deleteUserProfilePicture() {
    return this.http.delete(this.baseUrl + "/PublicUser/delete-profile-image");
  }

  updateUserAddress(updateModel: UpdateAddressDto) {
    return this.http.put(this.baseUrl + "/PublicUser/update-address", updateModel);
  }

  updateUserAdditionalInfo(updateModel: UpdateAddressDto) {
    return this.http.put(this.baseUrl + "/PublicUser/update-additional-info", updateModel);
  }

  updateMainInfo(updateModel: UpdateMainInfoDto) {
    return this.http.put(this.baseUrl + "/PublicUser/update-main-info", updateModel);
  }

  fetchUserUpdateData() : Observable<UserUpdateDataDto> {
    return this.http.get<UserUpdateDataDto>(this.baseUrl + "/PublicUser/fetch-update-data");
  }

  constructor() { }
}
