import { FormControl } from '@angular/forms';
import { email, notBlank, phone, trimmedLength } from './contact.validators';

function ctrl(value: unknown): FormControl {
  return new FormControl(value);
}

describe('contact validators', () => {
  describe('notBlank', () => {
    const v = notBlank();
    it('fails on whitespace-only', () => {
      expect(v(ctrl('   '))).toEqual({ blank: true });
    });
    it('passes on real content', () => {
      expect(v(ctrl('Ada'))).toBeNull();
    });
  });

  describe('trimmedLength', () => {
    const v = trimmedLength(2, 5);
    it('passes within range (trimmed)', () => {
      expect(v(ctrl('  abc  '))).toBeNull();
    });
    it('fails below min', () => {
      expect(v(ctrl('a'))?.['minLengthTrimmed']).toBeTruthy();
    });
    it('fails above max', () => {
      expect(v(ctrl('abcdef'))?.['maxLengthTrimmed']).toBeTruthy();
    });
    it('ignores empty (lets required handle it)', () => {
      expect(v(ctrl(''))).toBeNull();
    });
  });

  describe('email', () => {
    const v = email();
    it('accepts a valid address', () => {
      expect(v(ctrl('a@b.com'))).toBeNull();
    });
    it('rejects malformed addresses', () => {
      expect(v(ctrl('not-an-email'))).toEqual({ email: true });
      expect(v(ctrl('a@b'))).toEqual({ email: true });
      expect(v(ctrl('@b.com'))).toEqual({ email: true });
    });
    it('ignores empty', () => {
      expect(v(ctrl(''))).toBeNull();
    });
  });

  describe('phone', () => {
    const v = phone();
    it('accepts valid US format', () => {
      expect(v(ctrl('2025550100'))).toBeNull();
    });
    it('rejects non-US formats', () => {
      expect(v(ctrl('+1 202 555 0100'))).toEqual({ phone: true });
      expect(v(ctrl('(202) 555-0100'))).toEqual({ phone: true });
      expect(v(ctrl('202-555-0100'))).toEqual({ phone: true });
    });
    it('rejects junk', () => {
      expect(v(ctrl('abc'))).toEqual({ phone: true });
      expect(v(ctrl('123'))).toEqual({ phone: true });
    });
    it('is optional (empty passes)', () => {
      expect(v(ctrl(''))).toBeNull();
    });
  });
});
