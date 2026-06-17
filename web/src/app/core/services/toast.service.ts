import { Injectable, signal } from '@angular/core';

export type ToastVariant = 'success' | 'error';

export interface Toast {
  id: number;
  message: string;
  variant: ToastVariant;
  durationMs: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  readonly toasts = signal<Toast[]>([]);

  show(message: string, variant: ToastVariant = 'success', durationMs = 4000, onDismiss?: () => void): void {
    const id = ++this.nextId;
    this.toasts.update((list) => [...list, { id, message, variant, durationMs }]);
    setTimeout(() => {
      this.dismiss(id);
      onDismiss?.();
    }, durationMs);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }
}
