import {Component, forwardRef, input, signal} from '@angular/core';
import {NG_VALUE_ACCESSOR} from "@angular/forms";

@Component({
  selector: 'app-input-field',
  imports: [],
  templateUrl: './input-field.component.html',
  styleUrl: './input-field.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputFieldComponent),
      multi: true
    }
  ]
})
export class InputFieldComponent {
  label = input<string>('Label');
  type = input<string>('text');
  id = input<string>('custom-input-' + Math.random().toString(36).substring(2, 9));

  labelBgColor = input<string>("#FFFAFA")

  // Internal state tracking
  value = signal<string>('');
  disabled = signal<boolean>(false);

  // ControlValueAccessor callbacks
  onChange: any = () => {};
  onTouched: any = () => {};

  // When you type in the input, notify Angular's Form architecture
  onInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.value.set(val);
    this.onChange(val);
  }


  // Writes value from the parent form model into the component
  writeValue(val: any): void {
    this.value.set(val || '');
  }

  // Registers helper function to update parent form model on change
  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  // Registers helper function to update parent form model on touch/blur
  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  // Handles disabled state changes from parent form
  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }
}
