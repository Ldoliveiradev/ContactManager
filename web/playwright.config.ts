import { defineConfig, devices } from '@playwright/test';

/**
 * E2E config. Tests run against the running app — by default the Dockerized stack
 * on http://localhost:4300 (override with PLAYWRIGHT_BASE_URL, e.g. 4200 for the
 * dev server). Bring the stack up first: `docker compose up -d`.
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false, // tests share one seeded backend; run serially for determinism
  workers: 1,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: process.env['PLAYWRIGHT_BASE_URL'] ?? 'http://localhost:4300',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
