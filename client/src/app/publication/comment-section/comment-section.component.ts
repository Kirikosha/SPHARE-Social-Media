import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { CommentService } from '../../_services/comment.service';
import { AccountService } from '../../_services/account.service';
import { CommentModel, CreateCommentModel } from '../../_models/commentModel';
import { CommentCreateComponent } from '../comment-create/comment-create.component';
import { MakeComplaintComponent } from '../../complaint/make-complaint-component/make-complaint-component.component';
import { IconComponent } from "../../../shared-components/icon/icon.component";
import { Subject, takeUntil } from 'rxjs';
import { PaginationParams, PagedList } from '../../_models/shared/pagination/pagination';

@Component({
  selector: 'app-comment-section',
  standalone: true,
  imports: [CommonModule, CommentCreateComponent, MakeComplaintComponent, IconComponent],
  templateUrl: './comment-section.component.html',
  styleUrl: './comment-section.component.css'
})
export class CommentSectionComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() publicationId!: string;
  @Input() uniqueNameIdentifier!: string;

  private commentService = inject(CommentService);
  private router = inject(Router);
  accountService = inject(AccountService);
  private toastr = inject(ToastrService);
  private destroy$ = new Subject<void>();

  currentUniqueName?: string;
  comments: CommentModel[] = [];
  isLoading = true;
  errorLoading = false;
  isOwner = false;

  // Pagination state
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  hasNextPage = false;
  isScrollLoading = false;

  // Complaint modal
  @ViewChild('complaintModal') complaintComponent!: MakeComplaintComponent;
  @ViewChild('scrollSentinel') scrollSentinel!: ElementRef;
  
  complaintTargetId: string = "";
  complaintModalId = 'complaintModal-comment-section';
  
  private observer: IntersectionObserver | null = null;

  ngOnInit(): void {
    this.currentUniqueName = this.accountService.currentUser()?.uniqueNameIdentifier;
    this.isOwner = this.currentUniqueName === this.uniqueNameIdentifier;
    this.loadComments(true);
  }

  ngAfterViewInit(): void {
    this.setupInfiniteScroll();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.observer) {
      this.observer.disconnect();
    }
  }

  private setupInfiniteScroll(): void {
    if (this.observer) {
      this.observer.disconnect();
    }
    
    this.observer = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting && this.hasNextPage && !this.isScrollLoading && !this.isLoading) {
        this.loadMoreComments();
      }
    }, {
      rootMargin: '100px' // Trigger 100px before element is visible
    });
    
    if (this.scrollSentinel?.nativeElement) {
      this.observer.observe(this.scrollSentinel.nativeElement);
    }
  }

  loadComments(reset: boolean = false): void {
    if (reset) {
      this.currentPage = 1;
      this.comments = [];
      this.totalCount = 0;
      this.isLoading = true;
    } else {
      this.isScrollLoading = true;
    }
    this.errorLoading = false;

    const params: PaginationParams = {
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.commentService.getCommentsByPublicationId(this.publicationId, params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: PagedList<CommentModel>) => {
          const validComments = response.items.filter(comment => {
            if (!comment.author) {
              console.warn('Comment missing author:', comment);
              return false;
            }
            return true;
          });

          if (reset) {
            this.comments = validComments;
          } else {
            this.comments = [...this.comments, ...validComments];
          }
          
          this.totalCount = response.totalCount;
          this.hasNextPage = response.hasNextPage;
          this.currentPage = response.pageNumber;
          
          this.isLoading = false;
          this.isScrollLoading = false;
          
          // Re-setup observer after DOM updates
          if (!reset) {
            setTimeout(() => this.setupInfiniteScroll(), 0);
          }
        },
        error: (err) => {
          console.error('Error loading comments:', err);
          this.errorLoading = true;
          this.isLoading = false;
          this.isScrollLoading = false;
          if (reset) {
            this.toastr.error('Failed to load comments');
          } else {
            // Decrement page on error so retry works
            this.currentPage--;
          }
        }
      });
  }

  loadMoreComments(): void {
    if (this.hasNextPage && !this.isScrollLoading) {
      this.currentPage++;
      this.loadComments(false);
    }
  }

  onCommentCreated(comment: CreateCommentModel): void {
    this.commentService.createComment(comment).subscribe({
      next: (newComment) => {
        if (newComment.author) {
          // Prepend new comment and update total count
          this.comments = [newComment, ...this.comments];
          this.totalCount++;
        } else {
          console.error('New comment missing author:', newComment);
          this.toastr.error('Comment created but author data is missing');
        }
      },
      error: (err) => {
        this.toastr.error(err.message || 'Failed to create comment');
      }
    });
  }

  deleteComment(commentId: string): void {
    if (!confirm('Are you sure you want to delete this comment?')) return;

    this.commentService.deleteComment(commentId).subscribe({
      next: () => {
        this.comments = this.comments.filter(c => c.id !== commentId);
        this.totalCount = Math.max(0, this.totalCount - 1);
        this.toastr.success('Comment deleted');
      },
      error: () => {
        this.toastr.error('Failed to delete comment');
      }
    });
  }

  openThread(commentId: string): void {
    this.router.navigate(['/comments', commentId]);
  }

  isCommentOwner(uniqueNameIdentifier: string): boolean {
    return this.currentUniqueName === uniqueNameIdentifier;
  }

  onAdminCommentDeleted(id: string) {
    this.comments = this.comments.filter(p => p.id !== id);
    this.totalCount = Math.max(0, this.totalCount - 1);
  }

  allCommentsHaveAuthors(): boolean {
    return this.comments.every(comment => comment.author != null);
  }

  openComplaintModal(commentId: string): void {
    this.complaintTargetId = commentId;
    this.complaintComponent.openModal();
  }

  onComplaintSubmitted(): void {
    console.log('Complaint submitted for comment:', this.complaintTargetId);
  }

  onComplaintModalClosed(): void {
    this.complaintComponent.closeModal();
  }
}