import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly themeService = inject(ThemeService);

  protected readonly isAuthenticated = this.auth.isAuthenticated;
  protected readonly theme = this.themeService.theme;

  protected toggleTheme(): void {
    this.themeService.toggle();
  }

  protected logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
