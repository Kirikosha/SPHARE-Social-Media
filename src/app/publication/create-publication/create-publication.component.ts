import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { PublicationService } from '../../_services/publication.service';
import { CreatePublicationModel } from '../../_models/createPublicationModel';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-create-publication',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-publication.component.html',
  styleUrl: './create-publication.component.css'
})
export class CreatePublicationComponent {
  private toastrService = inject(ToastrService);
  private publicationService = inject(PublicationService);
  
  @ViewChild('contentSpan') contentSpan!: ElementRef<HTMLSpanElement>;
  
  publicationType: 'ordinary' | 'planned' = 'ordinary';
  scheduledDate: string = '';
  imagePreviews: string[] = [];
  selectedImages: File[] = [];
  isPosting: boolean = false;
  
  get minScheduleDate(): string {
    const now = new Date();
    // Add 1 hour to current time
    now.setHours(now.getHours() + 1);
    return now.toISOString().slice(0, 16);
  }

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
      this.toastrService.success('Post created successfully!');
      this.resetForm();
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
  
  handleImageUpload(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const files = Array.from(input.files);
      
      // Check total images don't exceed 4 (for example)
      if (this.selectedImages.length + files.length > 4) {
        this.toastrService.error('You can upload a maximum of 4 images');
        return;
      }
      
      files.forEach(file => {
        if (!file.type.match('image.*')) {
          this.toastrService.error('Only image files are allowed');
          return;
        }
        
        this.selectedImages.push(file);
        
        const reader = new FileReader();
        reader.onload = (e) => {
          this.imagePreviews.push(e.target?.result as string);
        };
        reader.readAsDataURL(file);
      });
      
      // Reset the input to allow selecting the same file again
      input.value = '';
    }
  }
  
  removeImage(index: number) {
    this.imagePreviews.splice(index, 1);
    this.selectedImages.splice(index, 1);
  }
  
  resetForm() {
    this.contentSpan.nativeElement.innerText = '';
    this.imagePreviews = [];
    this.selectedImages = [];
    this.publicationType = 'ordinary';
    this.scheduledDate = '';
    this.isPosting = false;
  }
}
