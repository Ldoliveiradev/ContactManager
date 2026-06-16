import { ChangeDetectionStrategy, Component } from '@angular/core';

/** A simple elevated surface container. Projects its content. */
@Component({
  selector: 'ui-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ng-content />`,
  styleUrl: './card.scss',
})
export class Card {}
