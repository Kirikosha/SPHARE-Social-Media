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
  model: any = {};

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  goToProfile() {
    this.router.navigateByUrl(`/profile/${this.accountService.currentUser()?.uniqueNameIdentifier}`);
  }
}
