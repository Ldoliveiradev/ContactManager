import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Label + projected control + optional error message. The actual <input> is
 * projected so the parent keeps full control of the reactive form binding.
 */
@Component({
  selector: 'ui-form-field',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './form-field.component.html',
  styleUrl: './form-field.component.scss',
})
export class FormFieldComponent {
  readonly label = input.required<string>();
  readonly for = input<string | null>(null);
  readonly optional = input(false);
  readonly error = input<string | null>(null);
  readonly hint = input<string | null>(null);
}
