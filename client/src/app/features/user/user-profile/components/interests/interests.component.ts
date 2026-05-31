import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-interests',
  standalone: true,
  imports: [],
  templateUrl: './interests.component.html',
  styleUrl: './interests.component.css'
})
export class InterestsComponent {
  @Input() interests: string[] = [];
}
