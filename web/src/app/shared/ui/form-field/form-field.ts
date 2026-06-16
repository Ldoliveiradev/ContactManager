import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Label + projected control + optional error message. The actual <input> is
 * projected so the parent keeps full control of the reactive form binding.
 */
@Component({
  selector: 'ui-form-field',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <label class="field">
      <span class="field__label">
        {{ label() }}
        @if (optional()) {
          <em class="field__optional">(optional)</em>
        }
      </span>
      <ng-content />
      @if (error()) {
        <small class="field__error">{{ error() }}</small>
      }
    </label>
  `,
  styleUrl: './form-field.scss',
})
export class FormField {
  readonly label = input.required<string>();
  readonly optional = input(false);
  readonly error = input<string | null>(null);
}
