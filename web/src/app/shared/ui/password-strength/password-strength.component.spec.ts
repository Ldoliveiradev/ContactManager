import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PasswordStrengthComponent } from './password-strength.component';

describe('PasswordStrengthComponent', () => {
  let fixture: ComponentFixture<PasswordStrengthComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PasswordStrengthComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PasswordStrengthComponent);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should show all requirements unmet when value is empty', () => {
    // Assert
    const met = fixture.nativeElement.querySelectorAll('.requirement--met');
    expect(met.length).toBe(0);
  });

  it('should mark length requirement met when value has 8+ chars', () => {
    // Arrange / Act
    fixture.componentRef.setInput('value', 'abcdefgh');
    fixture.detectChanges();

    // Assert
    const items = fixture.nativeElement.querySelectorAll('.requirement');
    expect(items[0].classList).toContain('requirement--met');
  });

  it('should mark all requirements met for a strong password', () => {
    // Arrange / Act
    fixture.componentRef.setInput('value', 'StrongPass1');
    fixture.detectChanges();

    // Assert
    const met = fixture.nativeElement.querySelectorAll('.requirement--met');
    expect(met.length).toBe(4);
  });

  it('should not mark uppercase requirement met when all lowercase', () => {
    // Arrange / Act
    fixture.componentRef.setInput('value', 'alllowercase1');
    fixture.detectChanges();

    // Assert
    const items = fixture.nativeElement.querySelectorAll('.requirement');
    expect(items[1].classList).not.toContain('requirement--met');
  });
});
