import { Directive, TemplateRef, inject, input } from '@angular/core';

/**
 * Marks an <ng-template> as the custom cell renderer for a given column key.
 * Usage: <ng-template uiGridCell="name" let-row>{{ row.name }}</ng-template>
 */
@Directive({ selector: '[uiGridCell]' })
export class GridCellDirective {
  /** The column key this template renders. */
  readonly uiGridCell = input.required<string>();
  readonly template = inject(TemplateRef);
}
