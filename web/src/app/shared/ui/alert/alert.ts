import { ChangeDetectionStrategy, Component } from '@angular/core';

/** Inline error/notice banner. Projects its message text. */
@Component({
  selector: 'ui-alert',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'alert' },
  template: `<ng-content />`,
  styleUrl: './alert.scss',
})
export class Alert {}
