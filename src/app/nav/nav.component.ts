import { Component, inject } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';
import {BsDropdownModule} from 'ngx-bootstrap/dropdown'
import { LoginModel } from '../_models/loginModel';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [RouterLink, FormsModule, BsDropdownModule, RouterLinkActive],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  accountService = inject(AccountService);
  private router = inject(Router);
  private toastr = inject(ToastrService);
  model: any = {};

  login() {
    console.log(this.model);
    this.accountService.login(this.model).subscribe({
      next: _ => {
        this.router.navigateByUrl('/');
      },
      error: error => this.toastr.error(error.error)
    })
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
