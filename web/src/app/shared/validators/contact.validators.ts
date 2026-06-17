import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Real-world contact validators, kept in sync with the backend rules
 * (Domain.Contact + DB column limits):
 *  - name: required, trimmed, 3..100 chars
 *  - email: required, RFC-ish format matching the server regex, max 200
 *  - phone: optional US format (XXX) XXX-XXXX, exactly 10 digits
 */

// Mirrors the server-side email check: local@domain.tld (no spaces).
const EMAIL_RE = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
// 10 raw digits — the mask directive stores digits, display is handled separately.
const PHONE_RE = /^\d{10}$/;

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

const PASSWORD_STRENGTH_RE = /(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/;

export function passwordStrength(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = (control.value ?? '') as string;
    if (value.length === 0) return null;
    return PASSWORD_STRENGTH_RE.test(value) ? null : { passwordStrength: true };
  };
}

export function passwordsMatch(passwordKey: string, confirmKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.get(passwordKey)?.value ?? '';
    const confirm = group.get(confirmKey)?.value ?? '';
    if (confirm.length === 0) return null;
    return password === confirm ? null : { passwordsMismatch: true };
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
