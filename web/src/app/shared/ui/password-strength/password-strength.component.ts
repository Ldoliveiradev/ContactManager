import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

interface Requirement {
  label: string;
  met: boolean;
}

@Component({
  selector: 'ui-password-strength',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './password-strength.component.html',
  styleUrl: './password-strength.component.scss',
})
export class PasswordStrengthComponent {
  readonly value = input('');

  protected readonly requirements = computed<Requirement[]>(() => {
    const v = this.value();
    return [
      { label: 'At least 8 characters',      met: v.length >= 8 },
      { label: 'One uppercase letter (A–Z)',  met: /[A-Z]/.test(v) },
      { label: 'One lowercase letter (a–z)',  met: /[a-z]/.test(v) },
      { label: 'One digit (0–9)',             met: /\d/.test(v) },
    ];
  });
}
