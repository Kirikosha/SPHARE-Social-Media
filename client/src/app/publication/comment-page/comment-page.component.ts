import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { CommentService } from '../../_services/comment.service';
import { AccountService } from '../../_services/account.service';
import { CommentModel, CreateCommentModel } from '../../_models/commentModel';
import { CommentCreateComponent } from '../comment-create/comment-create.component';
import { MakeComplaintComponent } from '../../complaint/make-complaint-component/make-complaint-component.component';
import { IconComponent } from "../../../shared-components/icon/icon.component";

@Component({
  selector: 'app-comment-page',
  standalone: true,
  imports: [CommonModule, RouterModule, CommentCreateComponent, MakeComplaintComponent, IconComponent],
  templateUrl: './comment-page.component.html',
  styleUrl: './comment-page.component.css'
})
export class CommentPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private commentService = inject(CommentService);
  accountService = inject(AccountService);
  private toastr = inject(ToastrService);

  parentCommentId!: string;
  parentComment?: CommentModel;
  replies: CommentModel[] = [];

  isLoadingParent = true;
  isLoadingReplies = true;
  errorLoadingParent = false;
  errorLoadingReplies = false;

  currentUniqueName?: string;

  // Complaint modal (single instance for the page)
  @ViewChild(MakeComplaintComponent) complaintComponent!: MakeComplaintComponent;
  complaintTargetId: string = "";
  complaintModalId = 'complaintModal-comment-page';

  ngOnInit(): void {
    this.currentUniqueName = this.accountService.currentUser()?.uniqueNameIdentifier;

    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      const id = idParam ? "" + idParam : null;

      if (!id || Number.isNaN(id)) {
        this.toastr.error('Invalid comment id');
        return;
      }

      this.parentCommentId = id;
      this.loadThread();
    });
  }

  loadThread(): void {
    this.parentComment = undefined;
    this.replies = [];

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
        this.loadReplies();
      },
      error: (err) => {
        console.error('Error loading parent comment:', err);
        this.errorLoadingParent = true;
        this.isLoadingParent = false;
        this.toastr.error('Failed to load comment');
      }
    });
  }

  private loadReplies(): void {
    this.isLoadingReplies = true;
    this.errorLoadingReplies = false;

    this.commentService.getReplies(this.parentCommentId).subscribe({
      next: (replies) => {
        const validReplies = replies.filter(r => {
          if (!r.author) {
            console.warn('Reply missing author:', r);
            return false;
          }
          return true;
        });

        this.replies = validReplies;
        this.isLoadingReplies = false;
      },
      error: (err) => {
        console.error('Error loading replies:', err);
        this.errorLoadingReplies = true;
        this.isLoadingReplies = false;
        this.toastr.error('Failed to load replies');
      }
    });
  }

  onReplyCreated(comment: CreateCommentModel): void {
    (comment as any).parentCommentId = this.parentCommentId;

    this.commentService.createComment(comment).subscribe({
      next: (newComment) => {
        if (newComment.author) {
          this.replies = [newComment, ...this.replies];
        } else {
          this.toastr.error('Reply created but author data is missing');
        }
      },
      error: (err) => {
        this.toastr.error(err.message || 'Failed to create reply');
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
        this.toastr.success('Comment deleted');
      },
      error: () => {
        this.toastr.error('Failed to delete comment');
      }
    });
  }

  openAsThread(commentId: string): void {
    this.router.navigate(['/comments', commentId]);
  }

  isCommentOwner(uniqueNameIdentifier: string): boolean {
    return this.currentUniqueName === uniqueNameIdentifier;
  }

  // ✅ Complaint
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
