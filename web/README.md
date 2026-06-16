# Contact Manager — Frontend

Angular 20 SPA for the Contact Manager app. For the full project (backend, Docker, data
model, API reference, demo credentials) see the [root README](../README.md).

## Tech

- Angular 20 (standalone components, signals, lazy-loaded routes)
- JWT auth via an HTTP interceptor + route guard
- SCSS design system (tokens / mixins / breakpoints) — no UI framework
- FontAwesome icons, Inter font, light/dark theme

## Structure

```text
src/app/
  core/          JWT interceptor, auth guard, theme service
  features/
    auth/        login + register — models/, services/, components/login/
    contacts/    contact CRUD — models/, services/, components/contact-list/ + contact-form/
    accounts/    profile + password — models/, services/, components/profile/
  shared/
    ui/          ui-button, ui-card, ui-alert, ui-form-field, ui-pagination,
                 ui-data-view (searchable / sortable / paginated card grid),
                 ui-skeleton (shimmer loading placeholder)
    pipes/       PhonePipe
    directives/  AutofocusDirective
    validators/  contact field validators
  styles/        SCSS design system: tokens, mixins, breakpoints
```

## Commands

```bash
npm install        # first time
npm start          # dev server at http://localhost:4200
npm run build      # production build to dist/
npm run test:ci    # unit tests (Karma + Jasmine, headless)
npm run e2e        # Playwright E2E (needs the stack running — see root README)
```

The API URL used in dev is configured in
[`src/environments/environment.ts`](src/environments/environment.ts). When served via the
Docker/nginx setup, the SPA uses a relative `/api` path proxied to the API container.
