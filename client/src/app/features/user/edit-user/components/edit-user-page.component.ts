import {Component, inject, OnInit} from '@angular/core';
import {EditProfileImageComponent} from "./edit-profile-image/edit-profile-image.component";
import {UserEditService} from "../services/user-edit.service";
import {UserUpdateDataDto} from "../models/update-user.dto";
import {ToastrService} from "ngx-toastr";
import {EditImageStates} from "../models/states";
import {EditAddressComponent} from "./edit-address/edit-address.component";
import {EditMainInfoComponent} from "./edit-main-info/edit-main-info.component";
import {EditDetailsComponent} from "./edit-details/edit-details.component";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-edit-user-page',
  standalone: true,
  imports: [
    EditProfileImageComponent,
    EditAddressComponent,
    EditMainInfoComponent,
    EditDetailsComponent,
    NgIf
  ],
  templateUrl: './edit-user-page.component.html',
  styleUrl: './edit-user-page.component.css'
})
export class EditUserPageComponent implements OnInit {
  private userEditService: UserEditService = inject(UserEditService);
  private toastrService: ToastrService = inject(ToastrService);
  userData: UserUpdateDataDto | null = null;
  detailsData: any = null;

  imageCacheBuster = '';

  ngOnInit(): void {
    this.loadUser();
  }

  loadUser() {
    this.userEditService.fetchUserUpdateData().subscribe({
      next: (data) => {
        this.userData = data;
        this.detailsData = {
          pronouns: data.pronouns,
          profileDescription: data.profileDescription,
          dateOfBirth: data.dateOfBirth,
          interests: data.interests
        };
      },
      error: err => {
        this.toastrService.error(err.message);
      }
    })
  }

  handleImageUpdateResult(result: EditImageStates) {
    switch (result) {
      case EditImageStates.deleted:
        if (this.userData) {
          this.userData.profileImageUrl = "assets/user.png";
        }
        break;
      case EditImageStates.updated:
        this.imageCacheBuster = `t=${new Date().getTime()}`;
        this.loadUser();
        break;
    }
  }

  get profileUrlWithCache(): string | null {
    if (!this.userData?.profileImageUrl) return null;

    const url = this.userData.profileImageUrl;
    if (!this.imageCacheBuster) return url;

    const separator = url.includes('?') ? '&' : '?';
    return `${url}${separator}${this.imageCacheBuster}`;
  }
}
