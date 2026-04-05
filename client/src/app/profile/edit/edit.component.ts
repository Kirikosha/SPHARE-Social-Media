import { Component, inject, OnInit } from '@angular/core';
import { MemberService } from '../../_services/member.service';
import { ToastrService } from 'ngx-toastr';
import { MemberModel } from '../../_models/user/memberModel';
import { UpdateMemberModel} from '../../_models/user/updateMemberModel';
import { FormBuilder, FormControl, FormGroup, FormsModule, NgForm, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../_services/account.service';
import { Router } from '@angular/router';
import { UpdateAddressModel } from '../../_models/user/updateAddressModel';
import { UpdateUserProfileDetailsModel } from '../../_models/user/updateUserProfileDetailsModel';
import { ageRangeValidator, maxArrayLength } from './edit-validation-util';

@Component({
  selector: 'app-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './edit.component.html',
  styleUrl: './edit.component.css'
})
export class EditComponent implements OnInit {
  private memberService = inject(MemberService);
  private accountService = inject(AccountService);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  
  memberModel!: MemberModel;
  editForm!: FormGroup;
  profileImagePreview?: string;
  isLoading = false;

  // For handling array inputs
  interestsInput = '';

  ngOnInit(): void {
    this.initializeForm();
    this.loadMember();
  }

initializeForm(): void {
  this.editForm = this.fb.group({
    username: ['', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(20),
      Validators.pattern(/^[a-zA-Z0-9_]+$/)
    ]],
    uniqueNameIdentifier: ['', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(20),
      Validators.pattern(/^[a-zA-Z0-9_]+$/)
    ]],
    profileImage: [null],
    
    // User Profile Details
    pronouns: [''],
    mainProfileDescription: [''],
    interests: [[], [maxArrayLength(20)]], // Array for interests
    interestsInput: [''], // Add this for the input field
dateOfBirth: ['', [ageRangeValidator(13, 130)]],

    
    // Address
    city: [''],
    country: ['']
  });
}

loadMember(): void {
  this.memberService.getMyProfile().subscribe({
    next: (member) => {
      this.memberModel = member;
      console.log(this.memberModel)
      console.log(member.userProfileDetails?.pronouns)
      this.editForm.patchValue({
        username: member.username,
        uniqueNameIdentifier: member.uniqueNameIdentifier,
        pronouns: member.userProfileDetails?.pronouns || '',
        mainProfileDescription: member.userProfileDetails?.mainProfileDescription || '',
        interests: member.userProfileDetails?.interests || [],
        interestsInput: member.userProfileDetails?.interests?.join(', ') || '', // Add this
        dateOfBirth: member.userProfileDetails?.dateOfBirth || '',
        city: member.address?.city || '',
        country: member.address?.country || ''
      });
      
      
      if (member.profileImage?.imageUrl) {
        this.profileImagePreview = member.profileImage.imageUrl;
      }
    },
    error: (err) => {
      this.toastr.error(err.error);
    }
  });
}

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length === 1) {
      const file = input.files[0];
      this.editForm.patchValue({ profileImage: file });
      this.editForm.get('profileImage')?.updateValueAndValidity();

      const reader = new FileReader();
      reader.onload = (e) => {
        this.profileImagePreview = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  onInterestsInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const interestsInput = input.value;
    
    const interestsArray = this.parseInterests(interestsInput);
    this.editForm.patchValue({ interests: interestsArray });
  }

  parseInterests(inputString: string): string[] {
    if (!inputString.trim()) return [];

    return inputString
      .split(',')
      .map(i => i.trim())
      .filter(i => i.length > 0)
      .slice(0, 20);
  }


  removeInterest(index: number): void {
    const interests = [...(this.editForm.value.interests || [])];
    interests.splice(index, 1);
    this.editForm.patchValue({ 
      interests: interests,
      interestsInput: interests.join(', ') 
    });
  }

  onSubmit(): void {
    if (this.editForm.invalid) return;

    this.isLoading = true;

    // Prepare user profile details if any fields are filled
    let userProfileDetails: UpdateUserProfileDetailsModel | undefined = undefined;
    const pronouns = this.editForm.value.pronouns?.trim();
    const description = this.editForm.value.mainProfileDescription?.trim();
    const dateOfBirth = this.editForm.value.dateOfBirth;
    const interests = this.editForm.value.interests || [];
    
    if (pronouns || description || dateOfBirth || interests.length > 0) {
      userProfileDetails = {
        pronouns: pronouns || undefined,
        mainProfileDescription: description || undefined,
        interests: interests,
        dateOfBirth: dateOfBirth || undefined
      };
    }

    // Prepare address if any fields are filled
    let address: UpdateAddressModel | undefined = undefined;
    const city = this.editForm.value.city?.trim();
    const country = this.editForm.value.country?.trim();
    
    if (city || country) {
      address = {
        city: city || undefined,
        country: country || undefined
      };
    }

    const updateModel: UpdateMemberModel = {
      id: this.memberModel.id,
      username: this.editForm.value.username,
      uniqueNameIdentifier: this.editForm.value.uniqueNameIdentifier,
      joinedAt: this.memberModel.joinedAt,
      profileImage: this.editForm.value.profileImage,
      blocked: this.memberModel.blocked,
      userProfileDetails: userProfileDetails,
      address: address
    };

    this.memberService.updateMember(updateModel).subscribe({
      next: () => {
        this.toastr.success('Profile updated successfully');
        this.isLoading = false;
        this.loadMember(); // Refresh data
        this.accountService.updateProfile({
          username: updateModel.username,
          uniqueNameIdentifier: updateModel.uniqueNameIdentifier,
          userId: updateModel.id,
          token: '',
          refreshToken: '',
          role: '',
          blocked: this.memberModel.blocked
        });
        this.router.navigate(['/profile', updateModel.uniqueNameIdentifier]);
      },
      error: (err) => {
        this.toastr.error('Failed to update profile', err.message);
        this.isLoading = false;
      }
    });
  }

  get username() { return this.editForm.get('username'); }
  get uniqueNameIdentifier() { return this.editForm.get('uniqueNameIdentifier'); }
  get pronouns() { return this.editForm.get('pronouns'); }
  get mainProfileDescription() { return this.editForm.get('mainProfileDescription'); }
  get dateOfBirth() { return this.editForm.get('dateOfBirth'); }
  get city() { return this.editForm.get('city'); }
  get country() { return this.editForm.get('country'); }

  formatDate(dateString: string): string {
    const [day, month, year] = dateString.split('.');
    const date = new Date(parseInt(year), parseInt(month) - 1, parseInt(day));
    return date.toLocaleDateString('en-US', { 
      month: 'long', 
      year: 'numeric' 
    });
  }

  get interestsInputControl() {
  return this.editForm.get('interestsInput') as FormControl;
}

get minBirthDate(): string {
  const d = new Date();
  d.setFullYear(d.getFullYear() - 130);
  return d.toISOString().split('T')[0];
}

get maxBirthDate(): string {
  const d = new Date();
  d.setFullYear(d.getFullYear() - 13);
  return d.toISOString().split('T')[0];
}

}