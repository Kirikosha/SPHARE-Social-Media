import {Component, inject, Input, OnInit} from '@angular/core';
import {CommonModule} from "@angular/common";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule
} from "@angular/forms";
import {InputFieldComponent} from "../../../../../shared/components/input-field/input-field.component";
import {UserEditService} from "../../services/user-edit.service";
import {UpdateAddressDto} from "../../models/update-user.dto";

@Component({
  selector: 'app-edit-address',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, InputFieldComponent],
  templateUrl: './edit-address.component.html',
  styleUrl: './edit-address.component.css'
})
export class EditAddressComponent implements OnInit {

  private _city = '';
  private _country = '';

  /* ── Fix: Accept string | null to match your database/API model type ── */
  @Input()
  set city(value: string | null) {
    this._city = value || '';
    this.updateFormValue('city', this._city);
  }
  get city(): string { return this._city; }

  @Input()
  set country(value: string | null) {
    this._country = value || '';
    this.updateFormValue('country', this._country);
  }
  get country(): string { return this._country; }

  private userEditService: UserEditService = inject(UserEditService);
  private fb: FormBuilder = inject(FormBuilder);

  addressForm!: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  ngOnInit(): void {
    // Completely clean of individual or group-level validators
    this.addressForm = this.fb.group({
      city: [this._city],
      country: [this._country]
    });
  }

  private updateFormValue(controlName: string, value: string): void {
    if (this.addressForm && this.addressForm.get(controlName)) {
      this.addressForm.get(controlName)?.setValue(value, { emitEvent: false });
    }
  }

  onSubmit(): void {
    // The form is always valid now, even if completely empty
    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    // If cleared, fallback to an empty string '' so the database knows to delete it
    const dto: UpdateAddressDto = {
      city: this.addressForm.value.city?.trim() || '',
      country: this.addressForm.value.country?.trim() || ''
    };

    this.userEditService.updateUserAddress(dto).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Address updated successfully.';
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || err.message || 'Something went wrong while saving your address.';
      }
    });
  }
}
