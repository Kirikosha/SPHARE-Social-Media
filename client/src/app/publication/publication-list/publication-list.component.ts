import { Component, inject, OnInit } from '@angular/core';
import { PublicationService } from '../../_services/publication.service';
import { mapPublicationToCard, PublicationModel } from '../../_models/publications/publicationModel';
import { AccountService } from '../../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { PublicationCardComponent } from "../publication-card/publication-card.component";
import { UpdatePublicationModel } from '../../_models/publications/updatePublicationModel';
import { ActivatedRoute } from '@angular/router';
import { CreateViolationComponent } from "../../admin/create-violation/create-violation.component";
import { PublicationCardModel } from '../../_models/publications/publicationCardModel';

@Component({
  selector: 'app-publication-my-list',
  standalone: true,
  imports: [CommonModule, PublicationCardComponent, CreateViolationComponent],
  templateUrl: './publication-list.component.html',
  styleUrl: './publication-list.component.css'
})
export class PublicationListComponent implements OnInit{
  private route = inject(ActivatedRoute);
  private publicationService = inject(PublicationService);
  accountService = inject(AccountService);
  private toastr = inject(ToastrService);
  
  publications: PublicationCardModel[] = [];
  isLoading = true;
  isCurrentUserProfile = false;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const uniqueNameIdentifier = params['uniqueNameIdentifier'];
      this.checkIfCurrentUser(uniqueNameIdentifier);
      this.loadPublications(uniqueNameIdentifier);
    });
  }

  private checkIfCurrentUser(uniqueNameIdentifier: string): void {
    const currentUser = this.accountService.currentUser();
    this.isCurrentUserProfile = currentUser?.uniqueNameIdentifier === uniqueNameIdentifier;
  }

loadPublications(uniqueNameIdentifier: string): void {
  this.isLoading = true;
  this.publicationService.getPublications(uniqueNameIdentifier).subscribe({
    next: (publications) => {
      const currentUser = this.accountService.currentUser();
      const isOwnProfile = currentUser?.uniqueNameIdentifier === uniqueNameIdentifier;
      const now = new Date();
      
      this.isLoading = false;
    },
    error: (error) => {
      this.toastr.error('Failed to load publications', error.message);
      this.isLoading = false;
    }
  });
}

onUpdatePublication(publication: UpdatePublicationModel): void {
  this.publicationService.updatePublication(publication).subscribe({
    next: (updatedCard: PublicationCardModel) => {
      const index = this.publications.findIndex(p => p.id === updatedCard.id);
      if (index !== -1) {
        this.publications[index] = updatedCard;
        this.toastr.success('Publication updated successfully');
      }
    },
    error: (error) => {
      this.toastr.error('Failed to update publication', error.message);
    }
  });
}

  onDeletePublication(publicationId: string): void {
    if (!confirm('Are you sure you want to delete this publication?')) return;
    
    this.publicationService.deletePublication(publicationId).subscribe({
      next: () => {
        this.publications = this.publications.filter(p => p.id !== publicationId);
        this.toastr.success('Publication deleted successfully');
      },
      error: (error) => {
        this.toastr.error('Failed to delete publication', error.message);
      }
    });
  }

  onLikePublication(publicationId: string): void {
    this.publicationService.likePublication(publicationId).subscribe({
      next: (likeResponse) => {
        const publication = this.publications.find(p => p.id === publicationId);
        if (publication) {
          publication.likesAmount = likeResponse.amountOfLikes;
          publication.isLikedByCurrentUser = likeResponse.isLikedByCurrentUser;
        }
      },
      error: (error) => {
        this.toastr.error('Failed to like publication', error.message);
      }
    });
  }

  onAdminPublicationDeleted(itemId: string): void {
    this.publications = this.publications.filter(p => p.id !== itemId);
  }
}
