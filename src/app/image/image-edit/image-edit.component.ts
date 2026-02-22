import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';

@Component({
  selector: 'app-image-edit',
  standalone: true,
  imports: [ImageCropperComponent, CommonModule],
  templateUrl: './image-edit.component.html',
  styleUrl: './image-edit.component.css'
})
export class ImageEditComponent implements OnChanges{
  @Input() file: File | undefined;
  @Input() index: number = 0; // position in the parent's image array
  @Output() cropApplied = new EventEmitter<{croppedFile: File; preview: string; index: number}>();
  @Output() cropCancelled = new EventEmitter<void>();
  @Output() close = new EventEmitter<void>();

  private cdr = inject(ChangeDetectorRef);

  imageChangedEvent: any = '';
  croppedImage: string = "";


  ngOnChanges(changes: SimpleChanges): void {
    if (changes['file'] && this.file) {
      this.initiateCropping(this.file);
    }
  }

  private initiateCropping(file: File): void {
    const syntheticEvent = {
      target: {
        files: [file],
        value: ''
      }
    };
    this.imageChangedEvent = syntheticEvent;
    this.croppedImage = '';
    this.cdr.detectChanges();
  }

  imageCropped(event: ImageCroppedEvent): void {
    if (event.base64) {
      this.croppedImage = event.base64;
    } 
    else if (event.blob) {
      const reader = new FileReader();
      reader.onload = () => {
        this.croppedImage = reader.result as string;
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(event.blob);
    }
    this.cdr.detectChanges();
  }

    async applyCrop(): Promise<void> {
    if (!this.croppedImage || !this.file) return;

    try {
      const blob = await fetch(this.croppedImage).then(res => res.blob());
      const croppedFile = new File([blob], this.file.name, {
        type: 'image/jpeg',
        lastModified: Date.now()
      });

      this.cropApplied.emit({
        croppedFile,
        preview: this.croppedImage,
        index: this.index
      });

      this.resetCropper();
      this.close.emit();
    } catch (error) {
      console.error('Error applying crop:', error);
      this.cancelCrop();
    }
  }

  cancelCrop(): void {
    this.resetCropper();
    this.cropCancelled.emit();
    this.close.emit();
  }

  private resetCropper(): void {
    this.imageChangedEvent = '';
    this.croppedImage = '';
  }

}
