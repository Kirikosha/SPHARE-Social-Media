import {Component, inject, Input, OnInit} from '@angular/core';
import {UserEditService} from "../../services/user-edit.service";
import {AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors} from "@angular/forms";
import {InputFieldComponent} from "../../../../../shared/components/input-field/input-field.component";
import {CommonModule, NgIf} from "@angular/common";

@Component({
  selector: 'app-edit-details',
  standalone: true,
  imports: [
    InputFieldComponent,
    ReactiveFormsModule,
    NgIf,
    CommonModule,
  ],
  templateUrl: './edit-details.component.html',
  styleUrl: './edit-details.component.css'
})
export class EditDetailsComponent implements OnInit {
  private userEditService: UserEditService = inject(UserEditService);
  private fb: FormBuilder = inject(FormBuilder)

  detailsForm!: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  interestsList: string[] = []

  private pastelColors = ['#E8F0FE', '#E6F4EA', '#FCE8E6', '#FFE5EC', '#FDF2E9', '#F3E8FF', '#E4F9F5'];
  maxDateString: string = '';

  @Input()
  set data(value: any) {
    console.log('data setter called');
    if (!value) return;

    this.interestsList = Array.isArray(value.interests) ? [...value.interests] : [];
    const initialInterestsText = this.interestsList.join(', ');
    let formattedDate = '';

    if (value.dateOfBirth) {
      const dateString = value.dateOfBirth;

      if (typeof dateString === 'string' && dateString.includes('.')) {
        const [day, month, year] = dateString.split('.');

        formattedDate = `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
      } else {
        const dateObj = new Date(dateString);

        if (!isNaN(dateObj.getTime())) {
          formattedDate = dateObj.toISOString().split('T')[0];
        }
      }
    }

    if (this.detailsForm) {
      this.detailsForm.patchValue({
        pronouns: value.pronouns || '',
        profileDescription: value.profileDescription || '',
        interestsInput: initialInterestsText,
        dateOfBirth: formattedDate
      }, { emitEvent: false });
    } else {
      this._initialValues = {
        pronouns: value.pronouns || '',
        profileDescription: value.profileDescription || '',
        interestsInput: initialInterestsText,
        dateOfBirth: formattedDate
      };
    }
  }

  private _initialValues = {
    pronouns: '',
    profileDescription: '',
    interestsInput: '',
    dateOfBirth: ''
  };

  ngOnInit() {
    this.calculateAgeLimit();

    this.detailsForm = this.fb.group({
      pronouns: [this._initialValues.pronouns],
      profileDescription: [this._initialValues.profileDescription],
      interestsInput: [this._initialValues.interestsInput],
      dateOfBirth: [this._initialValues.dateOfBirth, [this.dateOfBirthValidator]]
    });
  }

  get descriptionValue(): string {
    return this.detailsForm?.get('profileDescription')?.value || '';
  }

  get dateOfBirthControl() {
    return this.detailsForm?.get('dateOfBirth');
  }

  private calculateAgeLimit(): void {
    const today = new Date();
    const maxYear = today.getFullYear() - 14;
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    this.maxDateString = `${maxYear}-${month}-${day}`;
  }

  private dateOfBirthValidator = (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value) return null; // Field is optional

    const selectedDate = new Date(value);
    if (isNaN(selectedDate.getTime())) {
      return { invalidDate: true };
    }

    const maxDate = new Date(this.maxDateString);
    if (selectedDate > maxDate) {
      return { tooYoung: true };
    }

    if (selectedDate.getFullYear() < 1900) {
      return { invalidYear: true };
    }

    return null;
  };


  onInterestsInput(event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    this.syncInterestsFromInput(inputEl.value);
  }

  onInterestsKeyDown(event: KeyboardEvent): void {
    const inputEl = event.target as HTMLInputElement;

    if (event.key === 'Backspace' && !inputEl.value && this.interestsList.length > 0) {
      // Pop last committed tag back into the input for editing
      const lastTag = this.interestsList[this.interestsList.length - 1];
      const newInput = [...this.interestsList.slice(0, -1), lastTag].join(', ') + ', ';
      // Rebuild the text as all committed tags + last one restored (without trailing comma)
      const restored = this.interestsList.slice(0, -1).join(', ');
      const newValue = restored ? `${restored}, ${lastTag}` : lastTag;
      this.detailsForm.get('interestsInput')?.setValue(newValue, { emitEvent: false });
      this.syncInterestsFromInput(newValue);
      event.preventDefault();
    }
  }

  removeInterestTag(index: number): void {
    // Remove from list, then rebuild the input string from remaining committed tags
    const updated = [...this.interestsList];
    updated.splice(index, 1);

    const newValue = updated.join(', ');
    this.detailsForm.get('interestsInput')?.setValue(newValue, { emitEvent: false });
    this.syncInterestsFromInput(newValue);
  }

  private syncInterestsFromInput(raw: string): void {
    const parts = raw
      .split(',')
      .map(t => t.trim().toLowerCase())
      .filter(t => t.length > 0);

    const unique: string[] = [];

    for (const tag of parts) {
      if (!unique.includes(tag)) {
        unique.push(tag);

        if (unique.length >= 20) {
          break;
        }
      }
    }

    this.interestsList = unique;
  }

  getTagColor(index: number): string {
    return this.pastelColors[index % this.pastelColors.length];
  }

  onSubmit(): void {
    if (this.detailsForm.invalid) return;

    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    const rawDate = this.detailsForm.value.dateOfBirth;
    let finalDate: Date | undefined = undefined;

    if (rawDate) {
      finalDate = new Date(rawDate);
    }

    const dto = {
      pronouns: this.detailsForm.value.pronouns?.trim() || '',
      profileDescription: this.detailsForm.value.profileDescription?.trim() || '',
      interests: this.interestsList,
      dateOfBirth: finalDate
    };

    this.userEditService.updateUserAdditionalInfo(dto as any).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Profile details updated successfully.';
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || err.message || 'Something went wrong saving details.';
      }
    });
  }
}
