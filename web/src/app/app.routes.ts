import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'contacts' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login').then((m) => m.Login),
  },
  {
    path: 'contacts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/contacts/contact-list').then((m) => m.ContactList),
  },
  {
    path: 'contacts/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/contacts/contact-form').then((m) => m.ContactForm),
  },
  {
    path: 'contacts/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/contacts/contact-form').then((m) => m.ContactForm),
  },
  { path: '**', redirectTo: 'contacts' },
];
