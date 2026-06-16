import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AlertComponent } from './alert.component';

@Component({
  imports: [AlertComponent],
  template: `<ui-alert>Something went wrong</ui-alert>`,
})
class Host {}

describe('Alert', () => {
  it('projects content and has the alert role', () => {
    TestBed.configureTestingModule({ imports: [Host] });
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement.querySelector('ui-alert');
    expect(el.getAttribute('role')).toBe('alert');
    expect(el.textContent?.trim()).toBe('Something went wrong');
  });
});
