import {Component, inject, OnInit} from '@angular/core';
import {EditProfileImageComponent} from "./edit-profile-image/edit-profile-image.component";
import {UserEditService} from "../services/user-edit.service";
import {UserUpdateDataDto} from "../models/update-user.dto";
import {ToastrService} from "ngx-toastr";
import {EditImageStates} from "../models/states";
import {EditAddressComponent} from "./edit-address/edit-address.component";
import {EditMainInfoComponent} from "./edit-main-info/edit-main-info.component";
import {EditDetailsComponent} from "./edit-details/edit-details.component";

@Component({
  selector: 'app-edit-user-page',
  standalone: true,
  imports: [
    EditProfileImageComponent,
    EditAddressComponent,
    EditMainInfoComponent,
    EditDetailsComponent
  ],
  templateUrl: './edit-user-page.component.html',
  styleUrl: './edit-user-page.component.css'
})
export class EditUserPageComponent implements OnInit {
  private userEditService: UserEditService = inject(UserEditService);
  private toastrService: ToastrService = inject(ToastrService);
  userData: UserUpdateDataDto | null = null;

  ngOnInit(): void {
    this.loadUser();
  }

  loadUser() {
    this.userEditService.fetchUserUpdateData().subscribe({
      next: (data) => {
        this.userData = data;
        console.log('User Data Loaded:', this.userData);
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
          this.userData.ProfileImageUrl = "assets/user.png";
        }
        break;
      case EditImageStates.updated:
        if (this.userData && this.userData.ProfileImageUrl) {
          const timestamp = new Date().getTime();
          const separator = this.userData.ProfileImageUrl.includes('?') ? '&' : '?';

          this.userData.ProfileImageUrl = `${this.userData.ProfileImageUrl}${separator}v=${timestamp}`;
        }
    }
  }
}
