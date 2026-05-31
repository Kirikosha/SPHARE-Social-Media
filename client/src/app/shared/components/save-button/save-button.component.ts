import {Component, EventEmitter, Input, Output} from '@angular/core';

@Component({
  selector: 'app-save-button',
  imports: [],
  templateUrl: './save-button.component.html',
  styleUrl: './save-button.component.css'
})
export class SaveButtonComponent {
  @Input() disabled: boolean = false;

  @Output() actionClick = new EventEmitter<Event>();

  onClick(event: Event) : void {
    if (!this.disabled) {
      this.actionClick.emit(event)
    }
  }
}
