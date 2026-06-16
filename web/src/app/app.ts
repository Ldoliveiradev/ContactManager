import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faMoon, faRightFromBracket, faSun } from '@fortawesome/free-solid-svg-icons';
import { AuthService } from './features/auth/services/auth.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, FaIconComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly themeService = inject(ThemeService);

  protected readonly isAuthenticated = this.auth.isAuthenticated;
  protected readonly theme = this.themeService.theme;

  protected readonly faLogout = faRightFromBracket;
  protected readonly themeIcon = computed(() => (this.theme() === 'dark' ? faSun : faMoon));

  protected toggleTheme(): void {
    this.themeService.toggle();
  }

  protected logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
