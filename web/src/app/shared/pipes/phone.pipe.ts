import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats a US-style 10-digit phone number as (XXX) XXX-XXXX. Falls back to the
 * original value for anything that isn't exactly 10 digits (e.g. international
 * numbers already containing a "+"), and to a dash for empty values.
 */
@Pipe({ name: 'phone' })
export class PhonePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) {
      return '—';
    }

    const digits = value.replace(/\D/g, '');
    if (digits.length === 10) {
      return `(${digits.slice(0, 3)}) ${digits.slice(3, 6)}-${digits.slice(6)}`;
    }

    return value;
  }
}
