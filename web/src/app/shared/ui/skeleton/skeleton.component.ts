import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type SkeletonVariant = 'line' | 'circle' | 'rect';

@Component({
  selector: 'ui-skeleton',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './skeleton.component.html',
  styleUrl: './skeleton.component.scss',
})
export class SkeletonComponent {
  readonly variant = input<SkeletonVariant>('line');
  /** CSS width value, e.g. '60%', '120px'. Defaults to 100%. */
  readonly width = input<string>('100%');
  /** CSS height value. Defaults to 1em for lines, explicit for rect/circle. */
  readonly height = input<string>('');
}
