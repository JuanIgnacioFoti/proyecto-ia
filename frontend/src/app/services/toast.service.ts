import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'error' | 'success' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toasts = signal<Toast[]>([]);
  private nextId = 0;

  show(message: string, type: Toast['type'] = 'error', durationMs = 5000) {
    const id = ++this.nextId;
    this.toasts.update(ts => [...ts, { id, message, type }]);
    setTimeout(() => this.dismiss(id), durationMs);
  }

  dismiss(id: number) {
    this.toasts.update(ts => ts.filter(t => t.id !== id));
  }
}

/**
 * Transforms a validation error body `{"errors":{"field":["msg1","msg2"],...}}`
 * into a human-readable string.
 */
export function formatValidationErrors(errors: Record<string, string[]>): string {
  return Object.entries(errors)
    .flatMap(([, messages]) => messages)
    .join(' | ');
}
