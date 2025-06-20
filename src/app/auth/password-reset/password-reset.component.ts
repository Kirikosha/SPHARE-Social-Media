import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../_services/account.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-password-reset',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './password-reset.component.html',
  styleUrls: ['./password-reset.component.css']
})
export class PasswordResetComponent {
  resetForm: FormGroup;
  currentStep: 'email' | 'code' | 'password' = 'email';
  bsModalRef?: BsModalRef;
  countdown: number = 600; // 5 minutes in seconds
  countdownInterval: any;

  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private toastr: ToastrService,
    private router: Router,
  ) {
    this.resetForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      code: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(10)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validator: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  requestResetCode() {
    if (this.resetForm.get('email')?.invalid) {
      this.toastr.error('Please enter a valid email address');
      return;
    }

    const email = this.resetForm.get('email')?.value;
    this.accountService.forgotPassword(email).subscribe({
      next: () => {
        this.currentStep = 'code';
        this.startCountdown();
        this.toastr.success('Reset code sent to your email');
      },
      error: () => {
        this.currentStep = 'code';
        this.startCountdown();
        this.toastr.success('If an account exists, a reset code has been sent');
      }
    });
  }

  verifyCode() {
    if (this.resetForm.get('code')?.invalid) {
      this.toastr.error('Please enter the 10-digit code');
      return;
    }

    const email = this.resetForm.get('email')?.value;
    const code = this.resetForm.get('code')?.value;

    this.accountService.verifyResetCode(email, code).subscribe({
      next: () => {
        this.currentStep = 'password';
        this.stopCountdown();
        this.toastr.success('Code verified');
      },
      error: () => {
        this.toastr.error('Invalid or expired code');
      }
    });
  }

  resetPassword() {
    if (this.resetForm.invalid) {
      this.toastr.error('Please fill all fields correctly');
      return;
    }

    const email = this.resetForm.get('email')?.value;
    const newPassword = this.resetForm.get('newPassword')?.value;

    this.accountService.resetPassword(email, newPassword).subscribe({
      next: () => {
        this.toastr.success('Password reset successfully');
        this.router.navigateByUrl('/login');
      },
      error: () => {
        this.toastr.error('Failed to reset password');
      }
    });
  }

  startCountdown() {
    this.countdown = 300; // Reset to 5 minutes
    this.countdownInterval = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        this.stopCountdown();
        this.toastr.warning('Reset code has expired');
        this.currentStep = 'email';
      }
    }, 1000);
  }

  stopCountdown() {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }

  get countdownDisplay(): string {
    const minutes = Math.floor(this.countdown / 60);
    const seconds = this.countdown % 60;
    return `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
  }

  ngOnDestroy() {
    this.stopCountdown();
  }
}