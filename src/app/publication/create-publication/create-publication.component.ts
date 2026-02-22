import { Component, ElementRef, EventEmitter, inject, Output, ViewChild, ChangeDetectorRef } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { PublicationService } from '../../_services/publication.service';
import { CreatePublicationModel } from '../../_models/createPublicationModel';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
import { ImageEditComponent } from "../../image/image-edit/image-edit.component";

@Component({
  selector: 'app-create-publication',
  standalone: true,
  imports: [CommonModule, FormsModule, ImageEditComponent],
  templateUrl: './create-publication.component.html',
  styleUrl: './create-publication.component.css'
})
export class CreatePublicationComponent {
  @Output() publicationCreated = new EventEmitter<void>();
  private toastrService = inject(ToastrService);
  private publicationService = inject(PublicationService);
  private cdr = inject(ChangeDetectorRef);
  
  @ViewChild('contentSpan') contentSpan!: ElementRef<HTMLSpanElement>;
  @ViewChild('imageUpload') imageUpload!: ElementRef<HTMLInputElement>;

  publicationType: 'ordinary' | 'planned' = 'ordinary';
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
    
    if (this.publicationType === 'planned' && !this.scheduledDate) {
      this.toastrService.error("Please select a schedule date and time");
      return;
    }
    
    this.isPosting = true;

    const publication: CreatePublicationModel = {
      content: content,
      publicationType: this.publicationType,
      remindAt: this.publicationType === 'planned' ? new Date(this.scheduledDate) : undefined,
      images: this.selectedImages.length > 0 ? this.selectedImages : undefined
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
    // Add or replace image at the given index
    if (event.index >= this.selectedImages.length) {
      this.selectedImages.push(event.croppedFile);
      this.imagePreviews.push(event.preview);
    } else {
      this.selectedImages[event.index] = event.croppedFile;
      this.imagePreviews[event.index] = event.preview;
    }

    // Remove the processed file and close modal
    this.pendingFiles.shift();
    this.editingFile = null;

    // Continue with the next file (if any)
    this.startEditingNext();
  }

  onCropCancelled() {
    // Skip the current file and move to the next
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
    this.isPosting = false;
    if (this.imageUpload) {
      this.imageUpload.nativeElement.value = '';
    }
  }

}