import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { CommentModel } from '../../_models/commentModel';
import { PublicationModel } from '../../_models/publicationModel';
import { ToastrService } from 'ngx-toastr';
import { AdminService } from '../../_services/admin.service';
import { CommonModule, NgFor } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-create-violation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-violation.component.html',
  styleUrl: './create-violation.component.css'
})
export class CreateViolationComponent implements OnInit {
  accountService = inject(AccountService);
  @Input() item!: PublicationModel | CommentModel;
  @Output() itemDeleted = new EventEmitter<number>();
  private adminService = inject(AdminService);
  private toastr = inject(ToastrService);
  
  showModal = false;
  deletionReason = '';
  isDeleting = false;
  isAdmin = this.accountService.currentUser()?.role === 'Admin';
  isPublicationF = false;

  reasons = [
    'Spam or misleading content',
    'Abusive or harmful content',
    'Hate speech or symbols',
    'Violence or dangerous organizations',
    'Sexual content',
    'Intellectual property violation',
    'Privacy violation',
    'Other'
  ];

  ngOnInit(): void {
    this.isPublicationF = this.isPublication();
  }

  isPublication(): boolean {
    return 'commentAmount' in this.item;
  }  

  openModal(): void {
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.deletionReason = '';
  }

  confirmDeletion(): void {
    if (!this.deletionReason) {
      this.toastr.warning('Please select a reason for deletion');
      return;
    }

    var violationScoreIncrease = 1;
    if (this.isPublicationF){
      violationScoreIncrease = 4;
    }

    this.isDeleting = true;
    const violationData = {
      itemToRemoveId: this.item.id,
      removalReason: this.deletionReason,
      violationScoreIncrease: violationScoreIncrease
    };

    const deletion$ = this.isPublication() 
      ? this.adminService.deletePublication(violationData)
      : this.adminService.deleteComment(violationData);

    deletion$.subscribe({
      next: () => {
        this.toastr.success('Item deleted successfully');
        this.itemDeleted.emit(this.item.id);
        this.closeModal();
      },
      error: (err) => {
        this.toastr.error('Failed to delete item', err.message);
      },
      complete: () => {
        this.isDeleting = false;
      }
    });
  }
}
