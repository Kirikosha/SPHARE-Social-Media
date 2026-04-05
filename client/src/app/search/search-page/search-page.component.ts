import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MemberService } from '../../_services/member.service';
import { MemberModel } from '../../_models/user/memberModel';

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './search-page.component.html',
  styleUrl: './search-page.component.css'
})
export class SearchPageComponent {
  private memberService = inject(MemberService);

  searchQuery: string = '';
  searchResults: MemberModel[] = [];
  isLoading = false;
  hasSearched = false;

  onSearch() {
    if (!this.searchQuery.trim()){
      this.searchResults = [];
      this.hasSearched = false;
      return;
    }

    this.isLoading = true;
    this.hasSearched = true;

    this.memberService.searchForUser(this.searchQuery).subscribe({
      next: (data: MemberModel[]) => {
        this.searchResults = data;
        this.isLoading = false;
      },
      error: (error) => {
        this.searchResults = [];
        this.isLoading = false;
      }
    })
  }
}
