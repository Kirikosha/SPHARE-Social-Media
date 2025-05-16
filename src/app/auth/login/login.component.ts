import { Component, inject } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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
  loginForm = new FormGroup({
    email: new FormControl('', [
      Validators.required,
      Validators.email
    ]),
    password: new FormControl('', [
      Validators.required,
      Validators.minLength(8)
    ])
  })

  login(){
    if (this.loginForm.valid){
      const loginModel: LoginModel = {
        email: this.loginForm.value.email!.toLowerCase(),
        password: this.loginForm.value.password!
      }

      this.accountService.login(loginModel).subscribe({
        next: (() => {
          this.router.navigate(['/']);
        }),
        error: (err) => {

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
      })
    }
  }
}
