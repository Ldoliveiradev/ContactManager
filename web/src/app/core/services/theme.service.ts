import { Injectable, signal } from '@angular/core';

type Theme = 'light' | 'dark';
const THEME_KEY = 'cm_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly current = signal<Theme>(this.initial());
  readonly theme = this.current.asReadonly();

  constructor() {
    this.apply(this.current());
  }

  toggle(): void {
    this.set(this.current() === 'dark' ? 'light' : 'dark');
  }

  private set(theme: Theme): void {
    this.current.set(theme);
    localStorage.setItem(THEME_KEY, theme);
    this.apply(theme);
  }

  private apply(theme: Theme): void {
    document.documentElement.setAttribute('data-theme', theme);
  }

  private initial(): Theme {
    const stored = localStorage.getItem(THEME_KEY) as Theme | null;
    if (stored) {
      return stored;
    }
    // Fall back to the OS preference on first visit.
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
