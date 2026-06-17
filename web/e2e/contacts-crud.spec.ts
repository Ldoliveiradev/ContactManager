import { expect, test } from '@playwright/test';
import { register, uniqueUsername } from './helpers';

// Each test registers a fresh user so the contact list starts empty and isolated.
test.describe('Contact CRUD', () => {
  test('new user sees the empty state', async ({ page }) => {
    await register(page, uniqueUsername());
    await expect(page.getByText('No contacts yet.')).toBeVisible();
  });

  test('create a contact', async ({ page }) => {
    await register(page, uniqueUsername());
    await page.getByRole('button', { name: /New contact/i }).click();
    await expect(page.getByRole('heading', { name: 'New contact' })).toBeVisible();

    await page.getByLabel('Name').fill('Grace Hopper');
    await page.getByLabel('Email').fill('grace@example.com');
    await page.getByLabel('Phone').fill('2025550100');
    await page.getByRole('button', { name: 'Save' }).click();

    await expect(page.getByRole('heading', { name: 'Your contacts' })).toBeVisible();
    await expect(page.getByText('Grace Hopper')).toBeVisible();
    // Phone is formatted by the PhonePipe.
    await expect(page.getByText('(202) 555-0100')).toBeVisible();
  });

  test('validation: blank name and bad email block submit', async ({ page }) => {
    await register(page, uniqueUsername());
    await page.getByRole('button', { name: /New contact/i }).click();

    // Touch the name field and leave it blank, then enter a bad email.
    await page.getByLabel('Name').focus();
    await page.getByLabel('Email').fill('not-an-email');
    await page.getByLabel('Email').blur();

    // The invalid form keeps Save disabled (submission is blocked) and shows errors.
    await expect(page.getByRole('button', { name: 'Save' })).toBeDisabled();
    await expect(page.getByRole('heading', { name: 'New contact' })).toBeVisible();
    await expect(page.getByText('Name is required.')).toBeVisible();
    await expect(page.getByText('Enter a valid email address.')).toBeVisible();
  });

  test('edit a contact', async ({ page }) => {
    await register(page, uniqueUsername());
    // Create one first.
    await page.getByRole('button', { name: /New contact/i }).click();
    await page.getByLabel('Name').fill('Ada Lovelace');
    await page.getByLabel('Email').fill('ada@example.com');
    await page.getByRole('button', { name: 'Save' }).click();
    await expect(page.getByText('Ada Lovelace')).toBeVisible();

    await page.getByRole('button', { name: 'Edit Ada Lovelace' }).click();
    await expect(page.getByRole('heading', { name: 'Edit contact' })).toBeVisible();
    const name = page.getByLabel('Name');
    await name.fill('Ada L.');
    await page.getByRole('button', { name: 'Save' }).click();

    await expect(page.getByText('Ada L.')).toBeVisible();
  });

  test('delete a contact', async ({ page }) => {
    await register(page, uniqueUsername());
    await page.getByRole('button', { name: /New contact/i }).click();
    await page.getByLabel('Name').fill('Temp Person');
    await page.getByLabel('Email').fill('temp@example.com');
    await page.getByRole('button', { name: 'Save' }).click();
    await expect(page.getByText('Temp Person')).toBeVisible();

    // Confirm dialog auto-accept.
    page.on('dialog', (d) => d.accept());
    await page.getByRole('button', { name: 'Delete Temp Person' }).click();

    await expect(page.getByText('Temp Person')).toBeHidden();
    await expect(page.getByText('No contacts yet.')).toBeVisible();
  });
});
