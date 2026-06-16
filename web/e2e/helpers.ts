import { Page, expect } from '@playwright/test';

export const DEMO = { username: 'demo', password: 'Demo123!' };

let usernameCounter = 0;

/** A unique username so registration tests never collide, even across parallel workers. */
export function uniqueUsername(prefix = 'e2e'): string {
  // Combine time, a process-unique random base, and a monotonic counter so two
  // tests starting in the same millisecond on different workers cannot collide.
  const entropy = `${Date.now().toString(36)}${Math.floor(Math.random() * 1e9).toString(36)}`;
  return `${prefix}_${entropy}_${usernameCounter++}`;
}

/** Logs in via the UI and waits for the contacts page. */
export async function login(page: Page, username = DEMO.username, password = DEMO.password) {
  await page.goto('/auth');
  await page.getByLabel('Username').fill(username);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();
  await expect(page).toHaveURL(/\/contacts/);
}

/** Registers a brand-new user through the UI (ends authenticated on /contacts). */
export async function register(page: Page, username: string, password = 'Secret123!') {
  await page.goto('/auth');
  await page.getByRole('button', { name: /Register/i }).click();
  // Wait for the register view to render before filling its fields.
  await expect(page.getByRole('heading', { name: 'Contact Manager' })).toBeVisible();
  await expect(page.getByText('Create an account')).toBeVisible();
  await page.getByLabel('First name').fill('E2E');
  await page.getByLabel('Last name').fill('Tester');
  await page.getByLabel('Email').fill(`${username}@example.com`);
  await page.getByLabel('Username').fill(username);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: /^Register$/ }).click();
  await expect(page).toHaveURL(/\/contacts/);
}
