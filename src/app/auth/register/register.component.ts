import { Component, inject } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
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
  registerForm = new FormGroup({
    username: new FormControl('', [
      Validators.required,
      Validators.minLength(3)
    ]),
    email: new FormControl('', [
      Validators.required,
      Validators.email
    ]),
    password: new FormControl('', [
      Validators.required,
      Validators.minLength(8)
    ]),
    confirmPassword: new FormControl('', [
      Validators.required,
      Validators.minLength(8),
      this.matchValues('password')
      
    ])
  })

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : {isMatching: true}
    }
  }

  register(){
    if (this.registerForm.valid){
      const registerModel: RegisterModel = {
        username: this.registerForm.value.username!,
        email: this.registerForm.value.email!.toLowerCase(),
        password: this.registerForm.value.password!
      }

      this.accountService.register(registerModel).subscribe({
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
