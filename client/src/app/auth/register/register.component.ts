import { Component, inject } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { NgIf } from '@angular/common';
import { RegisterModel } from '../../_models/registerModel';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, RouterLink, RouterLinkActive],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private accountService = inject(AccountService);
  private toastrService = inject(ToastrService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  
  registerForm = this.fb.group({
    username: ['', [
      Validators.required,
      Validators.minLength(3)
    ]],
    email: ['', [
      Validators.required,
      Validators.email
    ]],
    password: ['', [
      Validators.required,
      Validators.minLength(8)
    ]],
    confirmPassword: ['', [
      Validators.required,
      this.matchValues('password')
    ]],
    image: [null as File | null]
  });

  imagePreview?: string;

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : { isMatching: true };
    };
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.registerForm.patchValue({ image: file });
      this.registerForm.get('image')?.updateValueAndValidity();

      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  register(): void {
    if (this.registerForm.valid) {
      const formData = new FormData();
      formData.append('Username', this.registerForm.value.username!);
      formData.append('Email', this.registerForm.value.email!.toLowerCase());
      formData.append('Password', this.registerForm.value.password!);
      
      if (this.registerForm.value.image) {
        formData.append('Image', this.registerForm.value.image);
      }

      this.accountService.register(formData).subscribe({
        next: () => {
          this.router.navigate(['/']);
        },
        error: (err) => {
          this.handleRegistrationError(err);
        }
      });
    }
  }

  private handleRegistrationError(err: any): void {
    const serverError = err.error;
    let errorMsg = 'Registration failed. Please try again.';

    if (typeof serverError === 'string') {
      errorMsg = serverError;
    } else if (serverError?.message) {
      errorMsg = serverError.message;
    } else if (serverError?.errors) {
      const messages = Object.values(serverError.errors).flat();
      errorMsg = messages.join('\n');
    }

    this.toastrService.error(errorMsg, 'Registration failed');
  }
}