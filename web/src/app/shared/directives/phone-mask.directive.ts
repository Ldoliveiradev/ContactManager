import { Directive, ElementRef, HostListener, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

/**
 * Formats a US phone number as the user types: (XXX) XXX-XXXX.
 * The form control value is always raw digits (e.g. "2025550100") —
 * the mask is purely a display concern. The API receives and stores digits only.
 */
@Directive({ selector: '[appPhoneMask]' })
export class PhoneMaskDirective {
  private readonly control = inject(NgControl);
  private readonly el = inject<ElementRef<HTMLInputElement>>(ElementRef);

  @HostListener('input')
  onInput(): void {
    const digits = this.el.nativeElement.value.replace(/\D/g, '').slice(0, 10);
    this.control.control?.setValue(digits, { emitEvent: true });
    this.el.nativeElement.value = this.format(digits);
  }

  /** Call this to populate the input from a raw-digits value (e.g. on edit load). */
  writeFormatted(digits: string): void {
    this.el.nativeElement.value = this.format(digits.replace(/\D/g, '').slice(0, 10));
  }

  static format(digits: string): string {
    if (digits.length === 0) return '';
    if (digits.length <= 3) return `(${digits}`;
    if (digits.length <= 6) return `(${digits.slice(0, 3)}) ${digits.slice(3)}`;
    return `(${digits.slice(0, 3)}) ${digits.slice(3, 6)}-${digits.slice(6)}`;
  }

  private format(digits: string): string {
    return PhoneMaskDirective.format(digits);
  }
}
