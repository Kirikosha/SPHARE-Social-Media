import { Component, inject } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LoginModel } from '../../_models/loginModel';
import { ToastrService } from 'ngx-toastr';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, RouterLink, RouterLinkActive],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private accountService = inject(AccountService);
  private toastrService = inject(ToastrService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  login(): void {
    if (this.loginForm.valid) {
      const loginModel: LoginModel = {
        email: this.loginForm.value.email!.toLowerCase(),
        password: this.loginForm.value.password!
      };

      this.accountService.login(loginModel).subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => this.handleLoginError(err)
      });
    }
  }

  private handleLoginError(err: any): void {
    const serverError = err.error;
    let errorMsg = 'Login failed. Please try again.';

    if (typeof serverError === 'string') {
      errorMsg = serverError;
    } else if (serverError?.message) {
      errorMsg = serverError.message;
    } else if (serverError?.errors) {
      const messages = Object.values(serverError.errors).flat();
      errorMsg = messages.join('\n');
    }

    this.toastrService.error(errorMsg, 'Login was not successful');
  }
}
