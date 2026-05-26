import { Component, inject, HostListener } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [RouterLink, FormsModule, BsDropdownModule, RouterLinkActive, CommonModule],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  accountService = inject(AccountService);
  private router = inject(Router);

  isScrolled = false;
  isMenuOpen = false;

  @HostListener('window:scroll')
  onScroll(): void {
    this.isScrolled = window.scrollY > 40;
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  logout(): void {
    this.accountService.logout();
    this.isMenuOpen = false;
    this.router.navigateByUrl('/');
  }

  goToProfile(): void {
    this.isMenuOpen = false;
    this.router.navigateByUrl(`/profile/${this.accountService.currentUser()?.uniqueNameIdentifier}`);
  }
}