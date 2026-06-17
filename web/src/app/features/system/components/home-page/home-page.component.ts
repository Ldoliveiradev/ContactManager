import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';
import { ButtonComponent } from '../../../../shared/ui/button/button.component';
import { CardComponent } from '../../../../shared/ui/card/card.component';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, ButtonComponent, CardComponent],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
})
export class HomePageComponent {
  private readonly auth = inject(AuthService);

  protected readonly isAuthenticated = this.auth.isAuthenticated;
  protected readonly primaryLink = computed(() => this.isAuthenticated() ? '/contacts' : '/auth');
  protected readonly primaryLabel = computed(() => this.isAuthenticated() ? 'Open contacts' : 'Sign in');
  protected readonly secondaryLink = computed(() => this.isAuthenticated() ? '/accounts' : '/auth');
  protected readonly secondaryLabel = computed(() => this.isAuthenticated() ? 'Open profile' : 'Create account');
}
