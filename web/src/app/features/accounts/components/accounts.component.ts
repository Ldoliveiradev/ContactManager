import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { AlertComponent } from '../../../shared/ui/alert/alert.component';
import { SkeletonComponent } from '../../../shared/ui/skeleton/skeleton.component';
import { AccountDto } from '../models/account-dto.interface';
import { AccountService } from '../services/account.service';
import { Profile } from './profile/profile.component';

@Component({
  selector: 'app-accounts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Profile, AlertComponent, SkeletonComponent],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss',
})
export class AccountsComponent implements OnInit {
  private readonly accountService = inject(AccountService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly account = signal<AccountDto | null>(null);

  ngOnInit(): void {
    this.accountService.getProfile().subscribe({
      next: (data) => {
        this.account.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load profile.');
        this.loading.set(false);
      },
    });
  }

  protected onProfileUpdated(data: AccountDto | null): void {
    this.account.set(data);
  }
}
