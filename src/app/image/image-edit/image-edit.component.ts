import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
import {EasyImageDrawing} from 'ngx-easy-image-drawing'

@Component({
  selector: 'app-image-edit',
  standalone: true,
  imports: [ImageCropperComponent, CommonModule, EasyImageDrawing],
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

  mode: 'crop' | 'draw' = 'crop';

  imageChangedEvent: any = '';
  croppedImage: string = "";

  drawingImageSrc: string | null = null;
  finalDrawingBlob: Blob | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['file'] && this.file) {
      this.mode = 'crop';
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
    this.finalDrawingBlob = null;
    this.drawingImageSrc = null;
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

  proceedToDraw(): void {
    if (!this.croppedImage) return;
    this.drawingImageSrc = this.croppedImage;
    this.finalDrawingBlob = null;
    this.mode = 'draw';
  }


  private dataURLtoBlob(dataURL: string): Blob {
    const byteString = atob(dataURL.split(',')[1]);
    const mimeString = dataURL.split(',')[0].split(':')[1].split(';')[0];
    const ab = new ArrayBuffer(byteString.length);
    const ia = new Uint8Array(ab);
    for (let i = 0; i < byteString.length; i++) {
      ia[i] = byteString.charCodeAt(i);
    }
    return new Blob([ab], { type: mimeString });
  }


  async applyCrop(): Promise<void> {
    if (this.mode === 'draw' && this.finalDrawingBlob && this.file) {
      const finalFile = new File([this.finalDrawingBlob], this.file.name, {
        type: 'image/png',
        lastModified: Date.now()
      });
      this.cropApplied.emit({
        croppedFile: finalFile,
        preview: URL.createObjectURL(finalFile),
        index: this.index
      });
      this.resetCropper();
      this.close.emit();
      return;
    }

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
  handleSavedImage(event: any): void {
  // If event is already a Blob
  if (event instanceof Blob) {
    this.finalDrawingBlob = event;
    return;
  }

  // If event is a string (dataURL)
  if (typeof event === 'string') {
    this.finalDrawingBlob = this.dataURLtoBlob(event);
    return;
  }

  // If event is an object with a 'dataURL' property (common pattern)
  if (event && typeof event === 'object' && 'dataURL' in event) {
    this.finalDrawingBlob = this.dataURLtoBlob(event.dataURL);
    return;
  }

  // If event is an object with a 'base64' property
  if (event && typeof event === 'object' && 'base64' in event) {
    this.finalDrawingBlob = this.dataURLtoBlob(event.base64);
    return;
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
