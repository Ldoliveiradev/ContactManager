import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Real-world contact validators, kept in sync with the backend rules
 * (Domain.Contact + DB column limits):
 *  - name: required, trimmed, 2..200 chars
 *  - email: required, RFC-ish format matching the server regex, max 200
 *  - phone: optional, but if present must look like a real phone number
 */

// Mirrors the server-side email check: local@domain.tld (no spaces).
const EMAIL_RE = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
// Accepts +, digits, spaces, dashes, parentheses; 7–20 digits total.
const PHONE_RE = /^[+]?[\d\s\-()]{7,20}$/;

/** Fails when the trimmed value is empty (catches whitespace-only input). */
export function notBlank(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = (control.value ?? '') as string;
    return value.trim().length === 0 ? { blank: true } : null;
  };
}

/** Validates the trimmed length is within [min, max]. */
export function trimmedLength(min: number, max: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const len = ((control.value ?? '') as string).trim().length;
    if (len === 0) {
      return null; // let required/notBlank own emptiness
    }
    if (len < min) {
      return { minLengthTrimmed: { requiredLength: min, actualLength: len } };
    }
    if (len > max) {
      return { maxLengthTrimmed: { requiredLength: max, actualLength: len } };
    }
    return null;
  };
}

export function email(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = ((control.value ?? '') as string).trim();
    if (value.length === 0) {
      return null;
    }
    return EMAIL_RE.test(value) ? null : { email: true };
  };
}

export function phone(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = ((control.value ?? '') as string).trim();
    if (value.length === 0) {
      return null; // optional
    }
    return PHONE_RE.test(value) ? null : { phone: true };
  };
}
