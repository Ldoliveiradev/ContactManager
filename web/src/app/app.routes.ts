import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'contacts' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/components/login/login.component').then((m) => m.Login),
  },
  {
    path: 'contacts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/contacts/components/contact-list/contact-list.component').then(
        (m) => m.ContactList,
      ),
  },
  {
    path: 'contacts/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/contacts/components/contact-form/contact-form.component').then(
        (m) => m.ContactForm,
      ),
  },
  {
    path: 'contacts/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/contacts/components/contact-form/contact-form.component').then(
        (m) => m.ContactForm,
      ),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/accounts/components/profile/profile.component').then((m) => m.Profile),
  },
  { path: '**', redirectTo: 'contacts' },
];
