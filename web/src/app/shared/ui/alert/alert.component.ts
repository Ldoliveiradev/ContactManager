import { ChangeDetectionStrategy, Component } from '@angular/core';

/** Inline error/notice banner. Projects its message text. */
@Component({
  selector: 'ui-alert',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'alert' },
  templateUrl: './alert.component.html',
  styleUrl: './alert.component.scss',
})
export class AlertComponent {}
