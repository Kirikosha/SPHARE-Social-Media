import { Component, inject, OnInit, AfterViewInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { PublicationService } from '../../_services/publication.service';
import { AccountService } from '../../_services/account.service';
import { mapPublicationToCard, PublicationModel } from '../../_models/publications/publicationModel';
import { UpdatePublicationModel } from '../../_models/publications/updatePublicationModel';
import { PublicationCardModel } from '../../_models/publications/publicationCardModel';
import { PublicationCardComponent } from "../publication-card/publication-card.component";
import { CreateViolationComponent } from "../../admin/create-violation/create-violation.component";
import { PagedList, PaginationParams } from '../../_models/shared/pagination/pagination';

@Component({
  selector: 'app-publication-my-list',
  standalone: true,
  imports: [CommonModule, PublicationCardComponent, CreateViolationComponent],
  templateUrl: './publication-list.component.html',
  styleUrl: './publication-list.component.css'
})
export class PublicationListComponent implements OnInit, AfterViewInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private publicationService = inject(PublicationService);
  accountService = inject(AccountService);
  private toastr = inject(ToastrService);

  publications: PublicationCardModel[] = [];
  isLoading = true;
  isLoadingMore = false;
  isCurrentUserProfile = false;
  hasNextPage = false;
  
  private currentPage = 1;
  private pageSize = 10;
  private uniqueNameIdentifier = '';
  private observer: IntersectionObserver | null = null;

  @ViewChild('scrollSentinel') scrollSentinel!: ElementRef;

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.uniqueNameIdentifier = params['uniqueNameIdentifier'];
      this.checkIfCurrentUser(this.uniqueNameIdentifier);
      this.resetAndLoad();
    });
  }

  ngAfterViewInit(): void {
    this.setupIntersectionObserver();
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  private checkIfCurrentUser(uniqueNameIdentifier: string): void {
    const currentUser = this.accountService.currentUser();
    this.isCurrentUserProfile = currentUser?.uniqueNameIdentifier === uniqueNameIdentifier;
  }

  private resetAndLoad(): void {
    this.publications = [];
    this.currentPage = 1;
    this.hasNextPage = false;
    this.isLoading = true;
    this.loadPublications(this.uniqueNameIdentifier, this.currentPage);
  }

  loadPublications(uniqueNameIdentifier: string, page: number): void {
    const params: PaginationParams = { page, pageSize: this.pageSize };
    
    this.publicationService.getPublications(uniqueNameIdentifier, params).subscribe({
      next: (response: PagedList<PublicationCardModel>) => {
        this.publications = page === 1 ? response.items : [...this.publications, ...response.items];
        this.hasNextPage = response.hasNextPage;
        this.isLoading = false;
        this.isLoadingMore = false;
      },
      error: (error) => {
        this.toastr.error('Failed to load publications', error.message);
        this.isLoading = false;
        this.isLoadingMore = false;
      }
    });
  }

  loadMore(): void {
    if (this.hasNextPage && !this.isLoadingMore && !this.isLoading) {
      this.isLoadingMore = true;
      this.currentPage++;
      this.loadPublications(this.uniqueNameIdentifier, this.currentPage);
    }
  }

  private setupIntersectionObserver(): void {
    this.observer = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting) {
        this.loadMore();
      }
    }, { rootMargin: '200px' });

    if (this.scrollSentinel?.nativeElement) {
      this.observer.observe(this.scrollSentinel.nativeElement);
    }
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