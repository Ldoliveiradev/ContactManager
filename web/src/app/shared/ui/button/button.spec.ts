import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Button } from './button';

@Component({
  imports: [Button],
  template: `<ui-button [variant]="variant" [type]="type" [disabled]="disabled">Save</ui-button>`,
})
class Host {
  variant: 'primary' | 'secondary' | 'ghost' = 'primary';
  type: 'button' | 'submit' = 'button';
  disabled = false;
}

describe('Button', () => {
  let fixture: ComponentFixture<Host>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
  });

  function btn(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('button');
  }

  it('renders a button with projected content', () => {
    expect(btn().textContent?.trim()).toBe('Save');
  });

  it('applies the variant class', () => {
    fixture.componentInstance.variant = 'secondary';
    fixture.detectChanges();
    expect(btn().classList).toContain('btn--secondary');
  });

  it('reflects the type input', () => {
    fixture.componentInstance.type = 'submit';
    fixture.detectChanges();
    expect(btn().type).toBe('submit');
  });

  it('disables the native button', () => {
    fixture.componentInstance.disabled = true;
    fixture.detectChanges();
    expect(btn().disabled).toBeTrue();
  });
});
