import { Component, inject, Input } from '@angular/core';
import { PublicationService } from '../../_services/publication.service';
import { PublicationModel } from '../../_models/publicationModel';
import { CommonModule, DatePipe, NgClass } from '@angular/common';

@Component({
  selector: 'app-publication-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './publication-card.component.html',
  styleUrl: './publication-card.component.css',
  providers: [DatePipe]
})
export class PublicationCardComponent {
  @Input() publication!: PublicationModel;

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
}
