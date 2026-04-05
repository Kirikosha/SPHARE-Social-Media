import { Component, EventEmitter, Input, Output, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { ComplaintService } from '../../_services/complaint.service';
import * as bootstrap from 'bootstrap';


@Component({
  selector: 'app-make-complaint',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './make-complaint-component.component.html',
  styleUrl: './make-complaint-component.component.css'
})
export class MakeComplaintComponent implements OnInit {
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService);
  private complaintService = inject(ComplaintService);
  
  @Input() targetId!: string;
  @Input() targetType: 'publication' | 'comment' = 'publication';
  @Input() title = 'Report Content';
  @Input() modalId = 'complaintModal';
  @Output() complaintSubmitted = new EventEmitter<void>();
  @Output() modalClosed = new EventEmitter<void>();

  complaintForm!: FormGroup;
  modalInstance: bootstrap.Modal | null = null;

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    this.complaintForm = this.fb.group({
      reason: ['', Validators.required],
      explanation: ['']
    });
  }

  openModal(): void {
    this.complaintForm.reset();
    const modalElement = document.getElementById(this.modalId);
    if (modalElement) {
      this.modalInstance = new bootstrap.Modal(modalElement);
      this.modalInstance.show();
      
      modalElement.addEventListener('hidden.bs.modal', () => {
        this.modalClosed.emit();
      });
    }
  }

  closeModal(): void {
    if (this.modalInstance) {
      this.modalInstance.hide();
    }
  }
submitComplaint(): void {
  if (this.complaintForm.valid) {
    const complaintData = {
      ...this.complaintForm.value,
      targetId: this.targetId,
      targetType: this.targetType
    };

    const request$ =
      this.targetType === 'comment'
        ? this.complaintService.makeCommentComplaint(complaintData)
        : this.complaintService.makePublicationComplaint(complaintData);

    request$.subscribe({
      next: () => {
        this.toastr.success('Report submitted successfully');
        this.closeModal();
        this.complaintSubmitted.emit();
      },
      error: (error) => {
        this.toastr.error('Failed to submit report');
        console.error('Error submitting complaint:', error);
      }
    });
  } else {
    this.complaintForm.markAllAsTouched();
  }
}

}