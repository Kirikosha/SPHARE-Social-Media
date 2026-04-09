import { Component, ElementRef, EventEmitter, inject, Output, ViewChild, ChangeDetectorRef, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { PublicationService } from '../../_services/publication.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
import { ImageEditComponent } from "../../image/image-edit/image-edit.component";
import { CreatePublicationModel } from '../../_models/publications/createPublicationModel';

@Component({
  selector: 'app-create-publication',
  standalone: true,
  imports: [CommonModule, FormsModule, ImageEditComponent],
  templateUrl: './create-publication.component.html',
  styleUrl: './create-publication.component.css'
})
export class CreatePublicationComponent implements OnChanges{
  @Input() prefillDateString?: string;
  @Output() publicationCreated = new EventEmitter<void>();
  private toastrService = inject(ToastrService);
  private publicationService = inject(PublicationService);
  private cdr = inject(ChangeDetectorRef);
  
  @ViewChild('contentSpan') contentSpan!: ElementRef<HTMLSpanElement>;
  @ViewChild('imageUpload') imageUpload!: ElementRef<HTMLInputElement>;

  publicationType: 'ordinary' | 'planned' | 'plannedConditional' = 'ordinary';
  conditionType: 'SubscriberCount' = 'SubscriberCount';
  conditionTarget: number | null = null;
  comparisonOperator: 'GreaterThanOrEqual' = 'GreaterThanOrEqual'; 
  scheduledDate: string = '';
  imagePreviews: string[] = [];
  selectedImages: File[] = [];
  isPosting: boolean = false;

  editingFile: { file: File; index: number } | null = null;
  pendingFiles: File[] = [];


  createPost(event: Event) {
    event.preventDefault();
    const content = this.contentSpan.nativeElement.innerText.trim();

    if (!content || content.length === 0) {
      this.toastrService.error("Please enter content for the post");
      return;
    }

    // Validation for conditional publications
    if (this.publicationType === 'plannedConditional') {
      if (!this.conditionTarget || this.conditionTarget <= 0) {
        this.toastrService.error("Please enter a valid target number for the condition");
        return;
      }
    } else if (this.publicationType === 'planned') {
      if (!this.scheduledDate) {
        this.toastrService.error("Please select a schedule date and time");
        return;
      }
      
      // Validate that scheduled date is at least 1 hour in the future
      const scheduledDateTime = new Date(this.scheduledDate);
      const minAllowedDate = new Date();
      minAllowedDate.setHours(minAllowedDate.getHours() + 1);
      
      if (scheduledDateTime < minAllowedDate) {
        this.toastrService.error("Scheduled time must be at least 1 hour in the future");
        return;
      }
    }

    this.isPosting = true;

    const publication: CreatePublicationModel = {
      content: content,
      publicationType: this.publicationType,
      // Only include remindAt if type is 'planned'
      remindAt: this.publicationType === 'planned' ? new Date(this.scheduledDate) : undefined,
      images: this.selectedImages.length > 0 ? this.selectedImages : undefined,
      // Include condition fields only for conditional
      conditionType: this.publicationType === 'plannedConditional' ? this.conditionType : undefined,
      conditionTarget: this.publicationType === 'plannedConditional' ? this.conditionTarget : undefined,
      comparisonOperator: this.publicationType === 'plannedConditional' ? this.comparisonOperator : undefined
    };

    this.publicationService.createPublication(publication).subscribe({
      next: (response) => {
        this.resetForm();
        this.publicationCreated.emit();
        this.toastrService.success('Publication created successfully!');
      },
      error: (err) => {
        console.error('Error details:', err);
        this.toastrService.error(err.error?.message || 'Failed to create post');
        this.isPosting = false;
      },
      complete: () => {
        this.isPosting = false;
      }
    });
  }


  get minScheduleDate(): string {
    const now = new Date();
    now.setHours(now.getHours() + 1);
    return now.toISOString().slice(0, 16);
  }

  handleImageUpload(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const files = Array.from(input.files);

      if (this.selectedImages.length + files.length > 4) {
        this.toastrService.error('You can upload a maximum of 4 images');
        input.value = '';
        return;
      }

      this.pendingFiles = [...files];
      this.startEditingNext();
    }
  }

    private startEditingNext() {
    if (this.pendingFiles.length === 0) {
      this.imageUpload.nativeElement.value = '';
      return;
    }

    const file = this.pendingFiles[0];
    // The index where this image will be placed (based on current array length)
    const index = this.selectedImages.length;

    if (!file.type.match('image.*')) {
      this.toastrService.error('Only image files are allowed');
      this.pendingFiles.shift();
      this.startEditingNext();
      return;
    }

    this.editingFile = { file, index };
  }


  onCropApplied(event: { croppedFile: File; preview: string; index: number }) {
    if (event.index >= this.selectedImages.length) {
      this.selectedImages.push(event.croppedFile);
      this.imagePreviews.push(event.preview);
    } else {
      this.selectedImages[event.index] = event.croppedFile;
      this.imagePreviews[event.index] = event.preview;
    }

    this.pendingFiles.shift();
    this.editingFile = null;

    this.startEditingNext();
  }

  onCropCancelled() {
    this.pendingFiles.shift();
    this.editingFile = null;
    this.startEditingNext();
  }

  removeImage(index: number): void {
    if (index > -1 && index < this.imagePreviews.length) {
      this.imagePreviews.splice(index, 1);
      this.selectedImages.splice(index, 1);
    }
  }


  resetForm() {
    this.contentSpan.nativeElement.innerText = '';
    this.imagePreviews = [];
    this.selectedImages = [];
    this.pendingFiles = [];
    this.publicationType = 'ordinary';
    this.scheduledDate = '';
    this.conditionTarget = null;               
    this.conditionType = 'SubscriberCount';
    this.isPosting = false;
    if (this.imageUpload) {
      this.imageUpload.nativeElement.value = '';
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['prefillDateString'] && this.prefillDateString) {
      this.publicationType = 'planned';
      const prefillDate = new Date(this.prefillDateString);
      const minAllowedDate = new Date();
      minAllowedDate.setHours(minAllowedDate.getHours() + 1);
      minAllowedDate.setMinutes(minAllowedDate.getMinutes() - 1);
      
      // Use prefill if valid, otherwise use default (now + 1h)
      if (prefillDate >= minAllowedDate) {
        this.scheduledDate = this.formatDateTimeLocal(prefillDate);
      } else {
        this.setDefaultScheduleDate();
      }
    }
  }

  onPublicationTypeChange(type: string) {
    if (type === 'planned' && !this.scheduledDate) {
      this.setDefaultScheduleDate();
    }
  }
  openDatePicker(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.showPicker && !input.disabled) {
      input.showPicker();
    }
  }

    private setDefaultScheduleDate() {
    const defaultDate = new Date();
    defaultDate.setHours(defaultDate.getHours() + 1);
    this.scheduledDate = this.formatDateTimeLocal(defaultDate);
  }

  // Helper to format Date to YYYY-MM-DDTHH:mm for datetime-local input
  private formatDateTimeLocal(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }
}