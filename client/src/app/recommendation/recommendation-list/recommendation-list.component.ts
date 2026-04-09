import { 
  Component, 
  inject, 
  OnInit, 
  viewChild, 
  ElementRef, 
  DestroyRef 
} from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { PublicationCardComponent } from '../../publication/publication-card/publication-card.component';
import { 
  RecommendationService, 
  PaginatedResponse 
} from '../../_services/recommendation.service';
import { PublicationModel } from '../../_models/publications/publicationModel';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PublicationCardModel } from '../../_models/publications/publicationCardModel';
import { UpdatePublicationModel } from '../../_models/publications/updatePublicationModel';

@Component({
  selector: 'app-recommendation-list',
  standalone: true,
  imports: [CommonModule, PublicationCardComponent],
  templateUrl: './recommendation-list.component.html',
  styleUrl: './recommendation-list.component.css'
})
export class RecommendationListComponent implements OnInit {
  private recommendationService = inject(RecommendationService);
  private toastr = inject(ToastrService);
  private destroyRef = inject(DestroyRef);
  
  // Template reference for infinite scroll sentinel
  private sentinel = viewChild<ElementRef>('sentinel');
  
  publications: PublicationModel[] = [];
  isLoading = true;
  isLoadingMore = false;
  hasMoreData = true;
  
  // Pagination state
  private currentPage = 1;
  private pageSize = 10;
  private intersectionObserver?: IntersectionObserver;

  ngOnInit(): void {
    this.loadPublications();
  }

  ngAfterViewInit(): void {
    this.setupIntersectionObserver();
  }

  private setupIntersectionObserver(): void {
    const options = {
      root: null,
      rootMargin: '100px',
      threshold: 0.1
    };

    this.intersectionObserver = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting && this.hasMoreData && !this.isLoadingMore && !this.isLoading) {
          this.loadMore();
        }
      });
    }, options);

    const sentinelEl = this.sentinel()?.nativeElement;
    if (sentinelEl) {
      this.intersectionObserver.observe(sentinelEl);
    }
  }

  loadPublications(reset: boolean = true): void {
    if (reset) {
      this.currentPage = 1;
      this.publications = [];
      this.hasMoreData = true;
    }

    this.isLoading = true;
    
    this.recommendationService.getRecommendations({
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response: PaginatedResponse<PublicationModel>) => {
          this.publications = reset 
            ? response.items 
            : [...this.publications, ...response.items];
          
          this.hasMoreData = response.hasNextPage;
          this.isLoading = false;
          this.isLoadingMore = false;
        },
        error: (error) => {
          this.toastr.error('Failed to load recommendations');
          this.isLoading = false;
          this.isLoadingMore = false;
        }
      });
  }

  loadMore(): void {
    if (this.isLoadingMore || !this.hasMoreData) return;
    
    this.isLoadingMore = true;
    this.currentPage++;
    
    this.recommendationService.getRecommendations({
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response: PaginatedResponse<PublicationModel>) => {
          this.publications = [...this.publications, ...response.items];
          this.hasMoreData = response.hasNextPage;
          this.isLoadingMore = false;
        },
        error: (error) => {
          this.toastr.error('Failed to load more recommendations');
          this.isLoadingMore = false;
          this.currentPage--; // Revert page increment on error
        }
      });
  }

  handlePublicationUpdate(updatedPublication: UpdatePublicationModel): void {
    const index = this.publications.findIndex(p => p.id === updatedPublication.id);
    if (index !== -1) {
      this.publications[index] = { 
        ...this.publications[index], 
        ...updatedPublication 
      };
    }
  }

  handlePublicationDelete(publicationId: string): void {
    this.publications = this.publications.filter(p => p.id !== publicationId);
  }

  handlePublicationLike(publicationId: string): void {
    const publication = this.publications.find(p => p.id === publicationId);
    if (publication) {
      publication.isLikedByCurrentUser = !publication.isLikedByCurrentUser;
      publication.likesAmount += publication.isLikedByCurrentUser ? 1 : -1;
    }
  }

  // Manual refresh that resets pagination
  refresh(): void {
    this.loadPublications(true);
  }

  ngOnDestroy(): void {
    if (this.intersectionObserver) {
      this.intersectionObserver.disconnect();
    }
  }


  private toPublicationCardModel(publication: PublicationModel): PublicationCardModel {
    return {
      ...publication,
      publicationType: publication.publicationType.toString()
    };
  }

  get publicationsForCard(): PublicationCardModel[] {
    return this.publications.map(pub => this.toPublicationCardModel(pub));
  }

}