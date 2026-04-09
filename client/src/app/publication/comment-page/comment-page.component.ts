// comment-page.component.ts - Key fixes highlighted
import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Subject, takeUntil } from 'rxjs';

import { CommentService } from '../../_services/comment.service';
import { AccountService } from '../../_services/account.service';
import { CommentModel, CreateCommentModel } from '../../_models/commentModel';
import { CommentCreateComponent } from '../comment-create/comment-create.component';
import { MakeComplaintComponent } from '../../complaint/make-complaint-component/make-complaint-component.component';
import { IconComponent } from "../../../shared-components/icon/icon.component";
import { PaginationParams, PagedList } from '../../_models/shared/pagination/pagination';

@Component({
  selector: 'app-comment-page',
  standalone: true,
  imports: [CommonModule, RouterModule, CommentCreateComponent, MakeComplaintComponent, IconComponent],
  templateUrl: './comment-page.component.html',
  styleUrl: './comment-page.component.css'
})
export class CommentPageComponent implements OnInit, AfterViewInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private commentService = inject(CommentService);
  accountService = inject(AccountService);
  private toastr = inject(ToastrService);
  private destroy$ = new Subject<void>();

  parentCommentId!: string;
  parentComment?: CommentModel;
  replies: CommentModel[] = [];

  isLoadingParent = true;
  isLoadingReplies = true;
  errorLoadingParent = false;
  errorLoadingReplies = false;

  currentUniqueName?: string;

  repliesCurrentPage = 1;
  repliesPageSize = 10;
  repliesTotalCount = 0;
  repliesHasNextPage = false;
  isRepliesScrollLoading = false;

  // Complaint modal
  @ViewChild(MakeComplaintComponent) complaintComponent!: MakeComplaintComponent;
  @ViewChild('repliesScrollSentinel') repliesScrollSentinel!: ElementRef;
  
  complaintTargetId: string = "";
  complaintModalId = 'complaintModal-comment-page';
  
  private repliesObserver: IntersectionObserver | null = null;

  ngOnInit(): void {
    this.currentUniqueName = this.accountService.currentUser()?.uniqueNameIdentifier;

    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      const id = idParam ? "" + idParam : null;

      if (!id) {
        this.toastr.error('Invalid comment id');
        return;
      }

      this.parentCommentId = id;
      this.loadThread();
    });
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.setupRepliesInfiniteScroll(), 100);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.repliesObserver) {
      this.repliesObserver.disconnect();
      this.repliesObserver = null;
    }
  }

  private setupRepliesInfiniteScroll(): void {
    if (this.repliesObserver) {
      this.repliesObserver.disconnect();
    }

    this.repliesObserver = new IntersectionObserver((entries) => {
      const entry = entries[0];
      
      console.log('🔍 Sentinel intersecting:', entry.isIntersecting, 
                  'hasNextPage:', this.repliesHasNextPage,
                  'isLoading:', this.isLoadingReplies,
                  'scrollLoading:', this.isRepliesScrollLoading);

      if (entry.isIntersecting && 
          this.repliesHasNextPage && 
          !this.isRepliesScrollLoading && 
          !this.isLoadingReplies) {
        
        console.log('📥 Loading more replies...');
        this.loadMoreReplies();
      }
    }, {
      root: null,
      rootMargin: '200px',
      threshold: 0
    });
    
    setTimeout(() => {
      if (this.repliesScrollSentinel?.nativeElement) {
        this.repliesObserver!.observe(this.repliesScrollSentinel.nativeElement);
        console.log('👁️ Observer attached to sentinel');
      }
    }, 0);
  }

  loadThread(): void {
    this.parentComment = undefined;
    this.replies = [];
    this.repliesCurrentPage = 1;
    this.repliesTotalCount = 0;
    this.repliesHasNextPage = false;

    this.isLoadingParent = true;
    this.isLoadingReplies = true;
    this.errorLoadingParent = false;
    this.errorLoadingReplies = false;

    this.loadParentComment();
  }

  private loadParentComment(): void {
    this.commentService.getComment(this.parentCommentId).subscribe({
      next: (comment) => {
        this.parentComment = comment;
        this.isLoadingParent = false;
        this.loadReplies(true);
      },
      error: (err) => {
        console.error('Error loading parent comment:', err);
        this.errorLoadingParent = true;
        this.isLoadingParent = false;
        this.toastr.error('Failed to load comment');
      }
    });
  }

  private loadReplies(reset: boolean = false): void {
    if (reset) {
      this.repliesCurrentPage = 1;
      this.replies = [];
      this.repliesTotalCount = 0;
      this.isLoadingReplies = true;
    } else {
      this.isRepliesScrollLoading = true;
    }
    this.errorLoadingReplies = false;

    const params: PaginationParams = {
      page: this.repliesCurrentPage,
      pageSize: this.repliesPageSize
    };

    console.log(`📡 Fetching replies: page=${params.page}, pageSize=${params.pageSize}`);

    this.commentService.getReplies(this.parentCommentId, params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: PagedList<CommentModel>) => {
          const validReplies = response.items.filter(r => {
            if (!r.author) {
              console.warn('Reply missing author:', r);
              return false;
            }
            return true;
          });

          if (reset) {
            this.replies = validReplies;
          } else {
            this.replies = [...this.replies, ...validReplies];
          }
          
          this.repliesTotalCount = response.totalCount;
          this.repliesHasNextPage = response.hasNextPage;
          this.repliesCurrentPage = response.pageNumber;
          
          this.isLoadingReplies = false;
          this.isRepliesScrollLoading = false;
        },
        error: (err) => {
          this.errorLoadingReplies = true;
          this.isLoadingReplies = false;
          this.isRepliesScrollLoading = false;
          if (reset) {
            this.toastr.error('Failed to load replies');
          } else {
            this.repliesCurrentPage = Math.max(1, this.repliesCurrentPage - 1);
          }
        }
      });
  }

  loadMoreReplies(): void {
    if (this.repliesHasNextPage && !this.isRepliesScrollLoading) {
      this.repliesCurrentPage++;
      this.loadReplies(false);
    }
  }

  onReplyCreated(comment: CreateCommentModel): void {
    (comment as any).parentCommentId = this.parentCommentId;

    this.commentService.createComment(comment).subscribe({
      next: (newComment) => {
        if (newComment.author) {
          this.replies = [newComment, ...this.replies];
          this.repliesTotalCount++;
        } else {
          this.toastr.error('Reply created but author data is missing');
        }
      },
      error: (err) => {
      }
    });
  }

  deleteComment(commentId: string, isParent: boolean): void {
    if (!confirm('Are you sure you want to delete this comment?')) return;

    this.commentService.deleteComment(commentId).subscribe({
      next: () => {
        if (isParent) {
          this.toastr.success('Comment deleted');
          this.router.navigateByUrl('/');
          return;
        }

        this.replies = this.replies.filter(c => c.id !== commentId);
        this.repliesTotalCount = Math.max(0, this.repliesTotalCount - 1);
        this.toastr.success('Comment deleted');
      },
      error: () => {
        this.toastr.error('Failed to delete comment');
      }
    });
  }

  goBackToPublication(): void {
    if (this.parentComment?.publicationId) {
      this.router.navigate(['/publication', this.parentComment.publicationId]);
    }
  }

  goBack(): void {
    window.history.back();
  }

  openAsThread(commentId: string): void {
    this.router.navigate(['/comments', commentId]);
  }

  isCommentOwner(uniqueNameIdentifier: string): boolean {
    return this.currentUniqueName === uniqueNameIdentifier;
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