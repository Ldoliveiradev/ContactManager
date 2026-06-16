import { expect, test } from '@playwright/test';
import { login } from './helpers';

// Uses the seeded demo user (15 contacts) for read-only search/sort/pagination.
test.describe('Data view: search, sort, pagination', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('paginates with 6 per page by default', async ({ page }) => {
    await expect(page.getByText('1–6 of 15')).toBeVisible();
    // 6 cards on the first page.
    await expect(page.locator('.grid-card')).toHaveCount(6);
  });

  test('navigates to page 2', async ({ page }) => {
    await page.getByRole('button', { name: '2', exact: true }).click();
    await expect(page.getByText('7–12 of 15')).toBeVisible();
  });

  test('changing page size to 24 shows all 15 on one page', async ({ page }) => {
    await page.getByLabel('Items per page').selectOption('24');
    await expect(page.locator('.grid-card')).toHaveCount(15);
  });

  test('search filters the list', async ({ page }) => {
    await page.getByLabel('Search').fill('Ada');
    await expect(page.locator('.grid-card')).toHaveCount(1);
    await expect(page.getByText('Ada Lovelace')).toBeVisible();
  });

  test('search with no matches shows the empty message', async ({ page }) => {
    await page.getByLabel('Search').fill('zzzznomatch');
    await expect(page.getByText('No contacts match your search.')).toBeVisible();
  });

  test('sort by name toggles ascending/descending', async ({ page }) => {
    await page.getByLabel('Items per page').selectOption('24');
    const sortName = page.getByRole('button', { name: /^Name/ });

    await sortName.click(); // ascending
    const firstAsc = await page.locator('.cell-name__text').first().textContent();
    expect(firstAsc?.trim()).toBe('Ada Lovelace');

    await sortName.click(); // descending
    const firstDesc = await page.locator('.cell-name__text').first().textContent();
    expect(firstDesc?.trim()).not.toBe('Ada Lovelace');
  });
});

test.describe('Theme', () => {
  test('dark-mode toggle flips the document theme', async ({ page }) => {
    await login(page);
    const html = page.locator('html');
    const before = await html.getAttribute('data-theme');

    await page.getByRole('button', { name: 'Toggle theme' }).click();
    const after = await html.getAttribute('data-theme');

    expect(after).not.toBe(before);
    expect(['light', 'dark']).toContain(after);
  });
});
