import {Component, inject, Input, OnInit} from '@angular/core';
import { CommonModule } from '@angular/common';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {UserEditService} from "../../services/user-edit.service";
import {UpdateMainInfoDto} from "../../models/update-user.dto";
import {InputFieldComponent} from "../../../../../shared/components/input-field/input-field.component";
@Component({
  selector: 'app-edit-main-info',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, InputFieldComponent],
  templateUrl: './edit-main-info.component.html',
  styleUrl: './edit-main-info.component.css'
})
export class EditMainInfoComponent implements OnInit {

  private _username = '';
  private _uniqueNameIdentifier = '';

  @Input()
  set username(value: string) {
    this._username = value || '';
    this.updateFormValue('username', this._username);
  }
  get username(): string { return this._username; }

  @Input()
  set uniqueNameIdentifier(value: string) {
    this._uniqueNameIdentifier = value || '';
    this.updateFormValue('uniqueNameIdentifier', this._uniqueNameIdentifier);
  }
  get uniqueNameIdentifier(): string { return this._uniqueNameIdentifier; }

  private userEditService: UserEditService = inject(UserEditService);
  private fb: FormBuilder = inject(FormBuilder);

  mainInfoForm!: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';
  suggestedOption: string | null = null;

  ngOnInit(): void {
    // Initialize form structure right away with whatever data is current
    this.mainInfoForm = this.fb.group({
      username: [this._username, [Validators.required, Validators.minLength(3)]],
      uniqueNameIdentifier: [this._uniqueNameIdentifier, [Validators.required, Validators.minLength(3)]]
    });
  }

  // ── 2. Helper function to feed data cleanly into the Form structure ──
  private updateFormValue(controlName: string, value: string): void {
    if (this.mainInfoForm && this.mainInfoForm.get(controlName)) {
      this.mainInfoForm.get(controlName)?.setValue(value, { emitEvent: false });
    }
  }

  get usernameControl() {
    return this.mainInfoForm.get('username');
  }

  get uniqueNameIdentifierControl() {
    return this.mainInfoForm.get('uniqueNameIdentifier');
  }

  applySuggestion(): void {
    if (this.suggestedOption) {
      this.mainInfoForm.patchValue({ uniqueNameIdentifier: this.suggestedOption });
      this.suggestedOption = null;
    }
  }

  dismissSuggestion(): void {
    this.suggestedOption = null;
  }

  onSubmit(): void {
    if (this.mainInfoForm.invalid) return;

    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';
    this.suggestedOption = null;

    const dto: UpdateMainInfoDto = {
      username: this.mainInfoForm.value.username,
      uniqueNameIdentifier: this.mainInfoForm.value.uniqueNameIdentifier
    };

    this.userEditService.updateMainInfo(dto).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Main info updated successfully.';
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 409 && err.error?.options) {
          const option = err.error.options;
          if (option.nameOption) {
            this.suggestedOption = option.nameOption;
          }
          this.errorMessage = 'That identifier is already taken. See the suggestion above.';
        } else {
          this.errorMessage = err.error?.detail || err.message || 'Something went wrong.';
        }
      }
    });
  }
}
