import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { PublicationModel } from '../../_models/publicationModel';
import { PublicationService } from '../../_services/publication.service';
import { CommonModule } from '@angular/common';
import { CommentSectionComponent } from "../comment-section/comment-section.component";
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-publication-page',
  standalone: true,
  imports: [CommonModule, CommentSectionComponent],
  templateUrl: './publication-page.component.html',
  styleUrl: './publication-page.component.css'
})
export class PublicationPageComponent {
  private accountService = inject(AccountService);
  private publicationService = inject(PublicationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toastr = inject(ToastrService);
  
  publication?: PublicationModel;
  isLoading = true;

  ngOnInit(): void {
    this.loadPublication();
  }

  loadPublication(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.toastr.error('Invalid publication ID');
      return;
    }

    this.publicationService.getPublicationById(+id).subscribe({
      next: publication => {
        this.publication = publication;
        console.log('Loaded publication:', this.publication);
        this.isLoading = false;
      },
      error: err => {
        this.toastr.error('Failed to load publication');
        this.isLoading = false;
      }
    });
  }

  getImageGridClass(imageCount: number): string {
    switch(imageCount) {
      case 1: return 'single-image';
      case 2: return 'two-images';
      case 3: return 'three-images';
      case 4: return 'four-images';
      default: return 'single-image';
    }
  }

  likePublication() {
    if (!this.publication || this.accountService.currentUser()?.blocked) return;

    this.publicationService.likePublication(this.publication.id).subscribe({
      next: (like) => {
        this.publication!.likesAmount = like.amountOfLikes;
        this.publication!.isLikedByCurrentUser = like.isLikedByCurrentUser;
      },
      error: (error) => {
        this.toastr.error('Failed to like publication', error.message);
      }
    });
  }
  goBack() {
    this.router.navigate([`/profile/${this.publication!.author.uniqueNameIdentifier}`])    
  }


}
