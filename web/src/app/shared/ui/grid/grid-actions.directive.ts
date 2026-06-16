import { Directive, TemplateRef, inject } from '@angular/core';

/**
 * Marks an <ng-template> as the per-row actions renderer.
 * Usage: <ng-template uiGridActions let-row> ...buttons... </ng-template>
 */
@Directive({ selector: '[uiGridActions]' })
export class GridActionsDirective {
  readonly template = inject(TemplateRef);
}
