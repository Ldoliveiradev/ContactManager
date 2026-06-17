import { Directive, TemplateRef, inject } from '@angular/core';

/**
 * Marks an <ng-template> as the per-row actions renderer.
 * Usage: <ng-template uiDataViewActions let-row> ...buttons... </ng-template>
 */
@Directive({ selector: '[uiDataViewActions]' })
export class DataViewActionsDirective {
  readonly template = inject(TemplateRef);
}
