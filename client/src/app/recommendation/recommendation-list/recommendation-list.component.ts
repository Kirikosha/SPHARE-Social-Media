import { Component, inject, OnInit } from '@angular/core';
import { PublicationService } from '../../_services/publication.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { PublicationCardComponent } from '../../publication/publication-card/publication-card.component';

@Component({
  selector: 'app-recommendation-list',
  standalone: true,
  imports: [CommonModule, PublicationCardComponent],
  templateUrl: './recommendation-list.component.html',
  styleUrl: './recommendation-list.component.css'
})
export class RecommendationListComponent implements OnInit {
  private publicationService = inject(PublicationService);
  private toastr = inject(ToastrService);
  publications: any[] = [];
  isLoading = true;

  loadPublications() {
    this.isLoading = true;
    this.publicationService.getRecommendations().subscribe({
      next: (publications) => {
        this.publications = publications;
        this.isLoading = false;
      },
      error: (error) => {
        this.toastr.error('Failed to load recommendations');
        this.isLoading = false;
      }
    });
  }

  handlePublicationUpdate(updatedPublication: any) {
    const index = this.publications.findIndex(p => p.id === updatedPublication.id);
    if (index !== -1) {
      this.publications[index] = updatedPublication;
    }
  }

  handlePublicationDelete(publicationId: string) {
    this.publications = this.publications.filter(p => p.id !== publicationId);
  }

  handlePublicationLike(publicationId: string) {
    const publication = this.publications.find(p => p.id === publicationId);
    if (publication) {
      publication.isLikedByCurrentUser = !publication.isLikedByCurrentUser;
      publication.likesAmount += publication.isLikedByCurrentUser ? 1 : -1;
    }
  }

  ngOnInit(): void {
    this.loadPublications();
  }
}