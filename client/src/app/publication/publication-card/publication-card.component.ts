import { Component, EventEmitter, inject, Input, Output, ViewChild } from '@angular/core';
import { CommonModule, DatePipe, NgClass } from '@angular/common';
import { UpdatePublicationModel } from '../../_models/publications/updatePublicationModel';
import { AccountService } from '../../_services/account.service';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MakeComplaintComponent } from "../../complaint/make-complaint-component/make-complaint-component.component";
import { IconComponent } from "../../../shared-components/icon/icon.component";
import { PublicationCardModel } from '../../_models/publications/publicationCardModel';

@Component({
  selector: 'app-publication-card',
  standalone: true,
  imports: [CommonModule, FormsModule, MakeComplaintComponent, IconComponent],
  templateUrl: './publication-card.component.html',
  styleUrl: './publication-card.component.css',
  providers: [DatePipe]
})
export class PublicationCardComponent {
  accountService = inject(AccountService);
  private router = inject(Router);
  @Input() publication!: PublicationCardModel;
  @Output() editPublication = new EventEmitter<UpdatePublicationModel>();
  @Output() deletePublication = new EventEmitter<string>();
  @Output() likePublication = new EventEmitter<string>();

  @ViewChild(MakeComplaintComponent) complaintComponent!: MakeComplaintComponent;

  isEditing = false;
  editedContent = '';
  editedRemindAt: string | null = null;
  constructor(private datePipe: DatePipe) {}


  formatDate(date: Date): string {
    return this.datePipe.transform(date, 'MMM d, y, h:mm a') || '';
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

  startEditing(){
    this.isEditing = true;
    this.editedContent = this.publication.content || '';
    this.editedRemindAt = this.publication.remindAt ? this.datePipe.transform(this.publication.remindAt, 'yyyy-MM-ddTHH:mm') : null;
  }

  cancelEditing(){
    this.isEditing = false;
  }

  getCurrentDateTime(): string {
    const now = new Date();
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`;
  }
  formatDateTime(date: Date): string {
    return this.datePipe.transform(date, 'MMM d, y, h:mm a') || '';
  }
  saveChanges() {
    const updatedPublication: UpdatePublicationModel = {
      id: this.publication.id,
      content: this.editedContent,
      remindAt: this.editedRemindAt ? new Date(this.editedRemindAt) : undefined
    };

    this.editPublication.emit(updatedPublication);
    this.isEditing = false;
  }

  delete() {
    if (confirm('Are you sure you want to delete this publication?')) {
      this.deletePublication.emit(this.publication.id);
    }
  }

  likePublicationMethod() {
    if (this.accountService.currentUser() && !this.accountService.currentUser()?.blocked) {
      this.likePublication.emit(this.publication.id);
    }
  }

  goToPublicationPage(publicationId: string) {
    this.router.navigate(['/publication', publicationId]);    
  }

  openComplaintModal(): void {
    this.complaintComponent.openModal();
  }

    onComplaintSubmitted(): void {
    console.log('Complaint submitted for publication:', this.publication.id);
    }

    onComplaintModalClosed(): void {
      this.complaintComponent.closeModal();
    }

  formatConditionOperator(operator?: 'GreaterThanOrEqual'): string {
    switch (operator) {
      case 'GreaterThanOrEqual': return '≥';
      default: return '';
  }
}
}
