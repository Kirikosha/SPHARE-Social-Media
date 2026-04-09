import { Component, inject } from '@angular/core';
import { PublicationCalendarModel } from '../../_models/publications/publicationCalendarModel';
import { PublicationService } from '../../_services/publication.service';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-calendar-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './calendar-page.component.html',
  styleUrl: './calendar-page.component.css'
})
export class CalendarPageComponent {
publications: PublicationCalendarModel[] = [];
private publicationService = inject(PublicationService)
  currentMonth: Date = new Date();
  daysInMonth: Date[] = [];


  ngOnInit() {
    this.fetchPublications();
  }

  fetchPublications() {
    this.publicationService.getPublicationCalendar().subscribe(data => {
      this.publications = data;
      this.buildCalendar();
    });
  }

  buildCalendar() {
    const year = this.currentMonth.getFullYear();
    const month = this.currentMonth.getMonth();
    const firstDayOfMonth = new Date(year, month, 1);
    const lastDayOfMonth = new Date(year, month + 1, 0);

    const startDayOfWeek = firstDayOfMonth.getDay(); // 0 = Sunday
    const startOffset = startDayOfWeek === 0 ? 6 : startDayOfWeek - 1; // for Monday start

    const totalDays = lastDayOfMonth.getDate();
    const rows = 6; // enough to cover any month
    const days: Date[] = [];

    // Build array of Date objects for each cell (including trailing)
    for (let i = 0; i < rows * 7; i++) {
      const day = new Date(year, month, 1 + i - startOffset);
      days.push(day);
    }
    this.daysInMonth = days;
  }

  // Helper to get publications for a specific day
  getPublicationsForDay(day: Date): PublicationCalendarModel[] {
    return this.publications.filter(p => {
      const pubDate = new Date(p.remindAt);
      return pubDate.toDateString() === day.toDateString();
    });
  }

  // Month navigation
  previousMonth() {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() - 1, 1);
    this.buildCalendar();
  }

  nextMonth() {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() + 1, 1);
    this.buildCalendar();
  }

  getLocalDateTimeString(day: Date): string {
    const year = day.getFullYear();
    const month = ('0' + (day.getMonth() + 1)).slice(-2);
    const dayOfMonth = ('0' + day.getDate()).slice(-2);
    return `${year}-${month}-${dayOfMonth}T12:00`;
  }
  isPastDate(day: Date): boolean {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    const compareDay = new Date(day);
    compareDay.setHours(0, 0, 0, 0);
    
    return compareDay < today;
  }
}
