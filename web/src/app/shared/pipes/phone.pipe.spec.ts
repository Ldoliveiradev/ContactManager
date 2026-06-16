import { PhonePipe } from './phone.pipe';

describe('PhonePipe', () => {
  const pipe = new PhonePipe();

  it('formats a 10-digit number', () => {
    expect(pipe.transform('2025550100')).toBe('(202) 555-0100');
  });

  it('strips non-digits before formatting', () => {
    expect(pipe.transform('202-555-0100')).toBe('(202) 555-0100');
  });

  it('returns a dash for empty values', () => {
    expect(pipe.transform(null)).toBe('—');
    expect(pipe.transform('')).toBe('—');
  });

  it('leaves non-10-digit values untouched', () => {
    expect(pipe.transform('+1-202-555-0100')).toBe('+1-202-555-0100');
  });
});
