import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TooltipDirective } from '../../directives/tooltip.directive';

type ButtonVariant = 'primary' | 'secondary' | 'ghost';
type ButtonType = 'button' | 'submit';

/**
 * Reusable button. Renders a real <button> and forwards click handling to the
 * host element, so usage stays `<ui-button (click)=...>`.
 */
@Component({
  selector: 'ui-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TooltipDirective],
  templateUrl: './button.component.html',
  styleUrl: './button.component.scss',
})
export class ButtonComponent {
  readonly variant = input<ButtonVariant>('primary');
  readonly type = input<ButtonType>('button');
  readonly disabled = input(false);
  readonly ariaLabel = input<string | null>(null, { alias: 'aria-label' });
  readonly tooltip = input<string | null>(null);
}
