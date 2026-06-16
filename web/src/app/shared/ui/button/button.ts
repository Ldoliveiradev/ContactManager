import { ChangeDetectionStrategy, Component, input } from '@angular/core';

type ButtonVariant = 'primary' | 'secondary' | 'ghost';
type ButtonType = 'button' | 'submit';

/**
 * Reusable button. Renders a real <button> and forwards click handling to the
 * host element, so usage stays `<ui-button (click)=...>`.
 */
@Component({
  selector: 'ui-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button [type]="type()" [disabled]="disabled()" [class]="'btn btn--' + variant()">
      <ng-content />
    </button>
  `,
  styleUrl: './button.scss',
})
export class Button {
  readonly variant = input<ButtonVariant>('primary');
  readonly type = input<ButtonType>('button');
  readonly disabled = input(false);
}
