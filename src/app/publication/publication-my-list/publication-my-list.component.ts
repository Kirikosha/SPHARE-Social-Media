import { Component, inject, OnInit } from '@angular/core';
import { PublicationService } from '../../_services/publication.service';
import { PublicationModel } from '../../_models/publicationModel';
import { AccountService } from '../../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { PublicationCardComponent } from "../publication-card/publication-card.component";

@Component({
  selector: 'app-publication-my-list',
  standalone: true,
  imports: [CommonModule, PublicationCardComponent],
  templateUrl: './publication-my-list.component.html',
  styleUrl: './publication-my-list.component.css'
})
export class PublicationMyListComponent implements OnInit{
  private publicationService = inject(PublicationService);
  private accountService = inject(AccountService);
  private toastr = inject(ToastrService);
  publications: PublicationModel[] = [];
  isLoading = true;
  isLoadingMore = false;
  currentPage = 1;
  pageSize = 10;
  hasMorePublications = true;

  ngOnInit(): void {
    this.publicationService.getPublications(this.accountService.currentUser()?.uniqueNameIdentifier!).subscribe({
      next: (publications) => {
        this.publications = publications;
        this.isLoading = false;
      },
      error: (error) => {
        this.toastr.error('Failed to load publications', error.message);
        this.isLoading = false;
      }
    })
  }
}
