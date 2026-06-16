import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormFieldComponent } from './form-field.component';

@Component({
  imports: [FormFieldComponent],
  template: `
    <ui-form-field [label]="label" [optional]="optional" [error]="error">
      <input type="text" />
    </ui-form-field>
  `,
})
class Host {
  label = 'Email';
  optional = false;
  error: string | null = null;
}

describe('FormFieldComponent', () => {
  let fixture: ComponentFixture<Host>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
  });

  it('renders the label and projects the input', () => {
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Email');
    expect(fixture.nativeElement.querySelector('input')).toBeTruthy();
  });

  it('shows the optional marker when set', () => {
    fixture.componentInstance.optional = true;
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('optional');
  });

  it('shows an error message when provided', () => {
    fixture.componentInstance.error = 'Required';
    fixture.detectChanges();
    const err = fixture.nativeElement.querySelector('.field__error');
    expect(err.textContent.trim()).toBe('Required');
  });

  it('hides the error element when there is no error', () => {
    expect(fixture.nativeElement.querySelector('.field__error')).toBeNull();
  });
});
