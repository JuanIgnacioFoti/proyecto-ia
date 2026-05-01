import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  styles: [`
    .toast-container {
      position: fixed;
      bottom: 1.5rem;
      right: 1.5rem;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      max-width: 380px;
    }
    .toast {
      padding: 0.75rem 1rem;
      border-radius: 8px;
      font-size: 0.875rem;
      font-family: var(--font-sans, sans-serif);
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 0.75rem;
      animation: slideIn 0.2s ease;
      line-height: 1.4;
      box-shadow: 0 4px 12px rgba(0,0,0,0.3);
    }
    @keyframes slideIn {
      from { transform: translateX(120%); opacity: 0; }
      to   { transform: translateX(0);   opacity: 1; }
    }
    .toast-error   { background: rgba(239,68,68,0.15); border: 1px solid rgba(239,68,68,0.4); color: #fca5a5; }
    .toast-success { background: rgba(16,185,129,0.15); border: 1px solid rgba(16,185,129,0.35); color: #6ee7b7; }
    .toast-info    { background: rgba(99,102,241,0.15); border: 1px solid rgba(99,102,241,0.35); color: #a5b4fc; }
    .toast-close {
      background: none; border: none; cursor: pointer;
      color: inherit; opacity: 0.7; font-size: 1rem; line-height: 1;
      padding: 0; flex-shrink: 0;
    }
    .toast-close:hover { opacity: 1; }
  `],
  template: `
    <div class="toast-container">
      @for (t of toastService.toasts(); track t.id) {
        <div class="toast toast-{{t.type}}">
          <span>{{ t.message }}</span>
          <button class="toast-close" (click)="toastService.dismiss(t.id)" aria-label="Dismiss">✕</button>
        </div>
      }
    </div>
  `
})
export class ToastComponent {
  constructor(public toastService: ToastService) {}
}
