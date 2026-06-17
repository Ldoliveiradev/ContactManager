import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AutofocusDirective } from './autofocus.directive';

@Component({
  imports: [AutofocusDirective],
  template: `<input appAutofocus />`,
})
class Host {}

describe('AutofocusDirective', () => {
  it('focuses the host element after init', async () => {
    TestBed.configureTestingModule({ imports: [Host] });
    const fixture = TestBed.createComponent(Host);
    // Attach to the document so focus() takes effect.
    document.body.appendChild(fixture.nativeElement);
    fixture.detectChanges();

    // Allow the queued microtask to run.
    await Promise.resolve();

    const input = fixture.nativeElement.querySelector('input');
    expect(document.activeElement).toBe(input);

    document.body.removeChild(fixture.nativeElement);
  });
});
