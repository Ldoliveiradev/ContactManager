import { expect, test } from '@playwright/test';
import { DEMO, login, register, uniqueUsername } from './helpers';

test.describe('Authentication', () => {
  test('unauthenticated user is redirected to login', async ({ page }) => {
    await page.goto('/contacts');
    await expect(page).toHaveURL(/\/login/);
  });

  test('demo user can log in and land on contacts', async ({ page }) => {
    await login(page);
    await expect(page.getByRole('heading', { name: 'Your contacts' })).toBeVisible();
  });

  test('wrong password shows an error and stays on login', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Username').fill(DEMO.username);
    await page.getByLabel('Password').fill('WrongPassword!');
    await page.getByRole('button', { name: 'Sign in' }).click();
    await expect(page.getByRole('alert')).toContainText(/invalid/i);
    await expect(page).toHaveURL(/\/login/);
  });

  test('a new user can register and is logged in', async ({ page }) => {
    await register(page, uniqueUsername());
    await expect(page.getByRole('heading', { name: 'Your contacts' })).toBeVisible();
  });

  test('logout returns to login and protects routes again', async ({ page }) => {
    await login(page);
    await page.getByRole('button', { name: /Log out/i }).click();
    await expect(page).toHaveURL(/\/login/);

    // Guard still active after logout.
    await page.goto('/contacts');
    await expect(page).toHaveURL(/\/login/);
  });
});
