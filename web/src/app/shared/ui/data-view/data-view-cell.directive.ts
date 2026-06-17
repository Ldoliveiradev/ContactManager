import { Directive, TemplateRef, inject, input } from '@angular/core';

/**
 * Marks an <ng-template> as the custom cell renderer for a given column key.
 * Usage: <ng-template uiDataViewCell="name" let-row>{{ row.name }}</ng-template>
 */
@Directive({ selector: '[uiDataViewCell]' })
export class DataViewCellDirective {
  /** The column key this template renders. */
  readonly uiDataViewCell = input.required<string>();
  readonly template = inject(TemplateRef);
}
