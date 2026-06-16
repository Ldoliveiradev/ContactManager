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
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/contacts/components/contact-list/contact-list.component').then(
            (m) => m.ContactList,
          ),
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./features/contacts/components/contact-form/contact-form.component').then(
            (m) => m.ContactForm,
          ),
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./features/contacts/components/contact-form/contact-form.component').then(
            (m) => m.ContactForm,
          ),
      },
    ],
  },
  {
    path: 'accounts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/accounts/components/accounts.component').then((m) => m.AccountsComponent),
    children: [
      {
        path: 'profile',
        loadComponent: () =>
          import('./features/accounts/components/profile/profile.component').then((m) => m.Profile),
      },
      { path: '', redirectTo: 'profile', pathMatch: 'full' },
    ],
  },
  { path: 'profile', redirectTo: 'accounts/profile', pathMatch: 'full' },
  { path: '**', redirectTo: 'contacts' },
];
