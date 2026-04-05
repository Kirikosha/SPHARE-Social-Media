import { Component, EventEmitter, Input, OnInit, Output, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { CommentService } from '../../_services/comment.service';
import { AccountService } from '../../_services/account.service';
import { CommentModel, CreateCommentModel } from '../../_models/commentModel';
import { CommentCreateComponent } from '../comment-create/comment-create.component';
import { MakeComplaintComponent } from '../../complaint/make-complaint-component/make-complaint-component.component';
import { IconComponent } from "../../../shared-components/icon/icon.component";

@Component({
  selector: 'app-comment-section',
  standalone: true,
  imports: [CommonModule, CommentCreateComponent, MakeComplaintComponent, IconComponent],
  templateUrl: './comment-section.component.html',
  styleUrl: './comment-section.component.css'
})
export class CommentSectionComponent implements OnInit {
  @Input() publicationId!: string;
  @Input() uniqueNameIdentifier!: string;

  private commentService = inject(CommentService);
  private router = inject(Router);
  accountService = inject(AccountService);
  private toastr = inject(ToastrService);

  currentUniqueName?: string;
  comments: CommentModel[] = [];
  isLoading = true;
  errorLoading = false;
  isOwner = false;

  // Complaint modal (single instance for the whole list)
@ViewChild('complaintModal') complaintComponent!: MakeComplaintComponent;
  complaintTargetId: string = "";
  complaintModalId = 'complaintModal-comment-section';

  ngOnInit(): void {
    this.currentUniqueName = this.accountService.currentUser()?.uniqueNameIdentifier;
    this.isOwner = this.currentUniqueName === this.uniqueNameIdentifier;
    this.loadComments();
  }

  loadComments(): void {
    this.isLoading = true;
    this.errorLoading = false;

    this.commentService.getCommentsByPublicationId(this.publicationId).subscribe({
      next: (comments) => {
        const validComments = comments.filter(comment => {
          if (!comment.author) {
            console.warn('Comment missing author:', comment);
            return false;
          }
          return true;
        });

        this.comments = validComments;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading comments:', err);
        this.errorLoading = true;
        this.isLoading = false;
        this.toastr.error('Failed to load comments');
      }
    });
  }

  onCommentCreated(comment: CreateCommentModel): void {
    this.commentService.createComment(comment).subscribe({
      next: (newComment) => {
        if (newComment.author) {
          this.comments = [newComment, ...this.comments];
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
