import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/system/components/home-page/home-page.component').then((m) => m.HomePageComponent),
  },
  {
    path: '401',
    loadComponent: () =>
      import('./features/system/components/unauthorized-page/unauthorized-page.component').then((m) => m.UnauthorizedPageComponent),
  },
  {
    path: '404',
    loadComponent: () =>
      import('./features/system/components/not-found-page/not-found-page.component').then((m) => m.NotFoundPageComponent),
  },
  {
    path: 'error',
    loadComponent: () =>
      import('./features/system/components/server-error-page/server-error-page.component').then((m) => m.ServerErrorPageComponent),
  },
  {
    path: 'auth',
    loadComponent: () =>
      import('./features/auth/components/auth.component').then((m) => m.AuthComponent),
  },
  { path: 'login', redirectTo: 'auth', pathMatch: 'full' },
  {
    path: 'contacts',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/contacts/components/contacts.component').then((m) => m.ContactsComponent),
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./features/contacts/components/contact-form/contact-form-page.component').then((m) => m.ContactFormPageComponent),
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./features/contacts/components/contact-form/contact-form-page.component').then((m) => m.ContactFormPageComponent),
      },
    ],
  },
  {
    path: 'accounts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/accounts/components/accounts.component').then((m) => m.AccountsComponent),
  },
  { path: 'profile', redirectTo: 'accounts', pathMatch: 'full' },
  { path: '**', redirectTo: '404' },
];
