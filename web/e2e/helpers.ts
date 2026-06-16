import { Page, expect } from '@playwright/test';

export const DEMO = { username: 'demo', password: 'Demo123!' };

/** A unique-ish username so registration tests don't collide across runs. */
export function uniqueUsername(prefix = 'e2e'): string {
  return `${prefix}_${Date.now()}_${Math.floor(Math.random() * 1e6)}`;
}

/** Logs in via the UI and waits for the contacts page. */
export async function login(page: Page, username = DEMO.username, password = DEMO.password) {
  await page.goto('/login');
  await page.getByLabel('Username').fill(username);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();
  await expect(page).toHaveURL(/\/contacts/);
}

/** Registers a brand-new user through the UI (ends authenticated on /contacts). */
export async function register(page: Page, username: string, password = 'Secret123!') {
  await page.goto('/login');
  await page.getByRole('button', { name: /Register/i }).click();
  await page.getByLabel('Username').fill(username);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: /^Register$/ }).click();
  await expect(page).toHaveURL(/\/contacts/);
}
