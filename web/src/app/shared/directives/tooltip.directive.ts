import { Directive, ElementRef, inject, input, effect } from '@angular/core';

/**
 * Sets a native browser tooltip (title attribute) on the host element.
 * Usage: <button [appTooltip]="form.invalid ? 'Fix errors first' : null">
 * A null/undefined value removes the attribute entirely.
 */
@Directive({ selector: '[appTooltip]' })
export class TooltipDirective {
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly appTooltip = input<string | null | undefined>(null);

  constructor() {
    effect(() => {
      const text = this.appTooltip();
      if (text) {
        this.el.nativeElement.setAttribute('title', text);
      } else {
        this.el.nativeElement.removeAttribute('title');
      }
    });
  }
}
