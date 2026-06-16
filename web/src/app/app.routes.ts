import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'contacts' },
  {
    path: 'auth',
    loadComponent: () =>
      import('./features/auth/components/auth.component').then((m) => m.AuthComponent),
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('./features/auth/components/login/login.component').then((m) => m.Login),
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' },
    ],
  },
  { path: 'login', redirectTo: 'auth/login', pathMatch: 'full' },
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
