import {Component, ElementRef, EventEmitter, inject, Input, Output, ViewChild} from '@angular/core';
import {UserEditService} from "../../services/user-edit.service";
import {ToastrService} from "ngx-toastr";
import {EditImageStates} from "../../models/states";
import {UpdateProfileImageDto} from "../../models/update-user.dto";
import {SaveButtonComponent} from "../../../../../shared/components/save-button/save-button.component";
import {NgClass, NgIf} from "@angular/common";

@Component({
  selector: 'app-edit-profile-image',
  standalone: true,
  imports: [
    SaveButtonComponent,
    NgIf,
    NgClass
  ],
  templateUrl: './edit-profile-image.component.html',
  styleUrl: './edit-profile-image.component.css'
})
export class EditProfileImageComponent {
  @Input() userProfileUrl: string | null = null;
  @Output() updateOperationResult: EventEmitter<EditImageStates> = new EventEmitter();
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  private userEditService: UserEditService = inject(UserEditService);
  private toastrService: ToastrService = inject(ToastrService);
  imagePreview?: string;
  pendingRemoval = false;
  selectedFile: File | null = null;

  deleteProfileImage() {
    if (this.userProfileUrl === "assets/user.png") {
      this.toastrService.info("User profile image was not set to be deleted.");
      return;
    }
    this.userEditService.deleteUserProfilePicture()
      .subscribe({
        next: () => {
          this.updateOperationResult.emit(EditImageStates.deleted);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'Failed to delete profile image';
          this.toastrService.error(errorMessage);
        }
      });
  }

  updateProfileImage() {
    if (!this.selectedFile) {
      this.toastrService.warning("Please select an image first");
      return;
    }

    const updateModel: UpdateProfileImageDto = {
      profileImage: this.selectedFile
    };

    this.userEditService.updateUserProfilePicture(updateModel)
      .subscribe({
        next: () => {
          this.imagePreview = undefined;
          this.selectedFile = null;
          this.updateOperationResult.emit(EditImageStates.updated);
        },
        error: (error) => {
          const errorMessage = error?.error?.message || error?.message || 'Failed to update profile image';
          this.toastrService.error(errorMessage);
        }
      })
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.[0]) {
      this.selectedFile = input.files[0];
      this.pendingRemoval = false;

      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview = e.target?.result as string;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }
  resetPreview(): void {
    this.selectedFile = null;
    this.imagePreview = undefined;
    this.fileInput.nativeElement.value = '';
  }

  get hasCustomImage(): boolean {
    return !!this.userProfileUrl && this.userProfileUrl !== 'assets/user.png';
  }
}
