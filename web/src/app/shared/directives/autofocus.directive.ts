import { AfterViewInit, Directive, ElementRef, inject } from '@angular/core';

/**
 * Focuses the host element after the view initializes. Handy for putting the
 * cursor in the first field of a form on load. Usage: <input appAutofocus />
 */
@Directive({ selector: '[appAutofocus]' })
export class AutofocusDirective implements AfterViewInit {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  ngAfterViewInit(): void {
    // Defer so it runs after the element is actually rendered/attached.
    queueMicrotask(() => this.host.nativeElement.focus());
  }
}
