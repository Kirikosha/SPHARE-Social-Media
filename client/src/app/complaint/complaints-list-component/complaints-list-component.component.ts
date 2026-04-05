import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ComplaintService } from '../../_services/complaint.service';
import { ToastrService } from 'ngx-toastr';
import { CommentComplaintModel } from '../../_models/complaints/commentComplaintModel';
import { GroupPublicationComplaintModel } from '../../_models/complaints/groupPublicationComplaintModel';
import { PublicationComplaintModel } from '../../_models/complaints/publicationComplaintModel';

@Component({
  selector: 'app-complaints-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './complaints-list-component.component.html',
  styleUrls: ['./complaints-list-component.component.css']
})
export class ComplaintsListComponent implements OnInit {
  private complaintService = inject(ComplaintService);
  private toastr = inject(ToastrService);

  activeTab: 'publication' | 'comment' | 'grouped' = 'publication';
  
  publicationComplaints: PublicationComplaintModel[] = [];
  commentComplaints: CommentComplaintModel[] = [];
  groupedComplaints: GroupPublicationComplaintModel[] = [];
  
  isLoading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.loadComplaints();
  }

  switchTab(tab: 'publication' | 'comment' | 'grouped'): void {
    this.activeTab = tab;
    
    // Load data for the tab if not already loaded
    switch(tab) {
      case 'publication':
        if (this.publicationComplaints.length === 0 && !this.isLoading) {
          this.loadPublicationComplaints();
        }
        break;
      case 'comment':
        if (this.commentComplaints.length === 0 && !this.isLoading) {
          this.loadCommentComplaints();
        }
        break;
      case 'grouped':
        if (this.groupedComplaints.length === 0 && !this.isLoading) {
          this.loadGroupedComplaints();
        }
        break;
    }
  }

  loadComplaints(): void {
    this.loadPublicationComplaints();
  }

  loadPublicationComplaints(): void {
    this.isLoading = true;
    this.error = null;
    
    this.complaintService.getPublicationComplaints().subscribe({
      next: (complaints) => {
        this.publicationComplaints = complaints;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load publication complaints';
        this.isLoading = false;
        this.toastr.error('Failed to load publication complaints');
      }
    });
  }

  loadCommentComplaints(): void {
    this.isLoading = true;
    this.error = null;
    
    this.complaintService.getCommentComplaints().subscribe({
      next: (complaints) => {
        this.commentComplaints = complaints;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load comment complaints';
        this.isLoading = false;
        this.toastr.error('Failed to load comment complaints');
      }
    });
  }

  loadGroupedComplaints(): void {
    this.isLoading = true;
    this.error = null;
    
    this.complaintService.getPublicationGroupedComplaints().subscribe({
      next: (complaints) => {
        this.groupedComplaints = complaints;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load grouped complaints';
        this.isLoading = false;
        this.toastr.error('Failed to load grouped complaints');
      }
    });
  }

  dismissPublicationComplaint(complaintId: number): void {
    if (confirm('Are you sure you want to dismiss this complaint?')) {
      // Implement dismissal logic here
      console.log('Dismissing complaint:', complaintId);
      this.publicationComplaints = this.publicationComplaints.filter(c => c.id !== complaintId);
      this.toastr.success('Complaint dismissed');
    }
  }

  dismissCommentComplaint(complaintId: number): void {
    if (confirm('Are you sure you want to dismiss this complaint?')) {
      // Implement dismissal logic here
      console.log('Dismissing comment complaint:', complaintId);
      this.commentComplaints = this.commentComplaints.filter(c => c.id !== complaintId);
      this.toastr.success('Complaint dismissed');
    }
  }

  deletePublication(publicationId: number, complaintId?: number): void {
    if (confirm('Are you sure you want to delete this publication? This action cannot be undone.')) {
      // Implement delete logic here
      console.log('Deleting publication:', publicationId);
      
      if (complaintId) {
        this.publicationComplaints = this.publicationComplaints.filter(c => c.id !== complaintId);
      }
      this.toastr.success('Publication deleted');
    }
  }

  deleteComment(commentId: number, complaintId?: number): void {
    if (confirm('Are you sure you want to delete this comment? This action cannot be undone.')) {
      // Implement delete logic here
      console.log('Deleting comment:', commentId);
      
      if (complaintId) {
        this.commentComplaints = this.commentComplaints.filter(c => c.id !== complaintId);
      }
      this.toastr.success('Comment deleted');
    }
  }
}