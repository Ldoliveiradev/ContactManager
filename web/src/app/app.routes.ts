import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'contacts' },
  {
    path: 'auth',
    loadComponent: () =>
      import('./features/auth/components/auth.component').then((m) => m.AuthComponent),
  },
  { path: 'login', redirectTo: 'auth', pathMatch: 'full' },
  {
    path: 'contacts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/contacts/components/contacts.component').then((m) => m.ContactsComponent),
  },
  {
    path: 'accounts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/accounts/components/accounts.component').then((m) => m.AccountsComponent),
  },
  { path: 'profile', redirectTo: 'accounts', pathMatch: 'full' },
  { path: '**', redirectTo: 'contacts' },
];
