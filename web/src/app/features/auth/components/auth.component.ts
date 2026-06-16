import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { Login } from './login/login.component';
import { Register } from './register/register.component';

@Component({
  selector: 'app-auth',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Login, Register],
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.scss',
})
export class AuthComponent {
  protected readonly view = signal<'login' | 'register'>('login');
}
