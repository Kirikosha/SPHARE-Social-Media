import { AbstractControl, ValidationErrors } from '@angular/forms';

export function maxArrayLength(max: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as any[];
    if (!Array.isArray(value)) return null;
    return value.length > max ? { maxArrayLength: { max, actual: value.length } } : null;
  };
}


export function ageRangeValidator(minAge: number, maxAge: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;

    const dob = new Date(control.value);
    const today = new Date();

    let age = today.getFullYear() - dob.getFullYear();
    const m = today.getMonth() - dob.getMonth();

    if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) {
      age--;
    }

    if (age < minAge) {
      return { tooYoung: true };
    }

    if (age > maxAge) {
      return { tooOld: true };
    }

    return null;
  };
}
