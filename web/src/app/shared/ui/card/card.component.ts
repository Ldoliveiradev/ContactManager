import { ChangeDetectionStrategy, Component } from '@angular/core';

/** A simple elevated surface container. Projects its content. */
@Component({
  selector: 'ui-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './card.component.html',
  styleUrl: './card.component.scss',
})
export class CardComponent {}
