import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  styles: [`
    :host { display: block; }
    .auth-subtitle {
      text-align: center;
      color: var(--color-text-faint);
      font-size: 0.875rem;
      margin-top: -0.75rem;
      margin-bottom: 1.5rem;
    }
    .divider {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin: 0.25rem 0;
      color: var(--color-text-faint);
      font-size: 0.75rem;
    }
    .divider::before,
    .divider::after {
      content: '';
      flex: 1;
      height: 1px;
      background: var(--color-border);
    }
  `],
  template: `
    <div class="auth-container">
      <h2>Login</h2>
      <p class="auth-subtitle">Welcome back to the platform</p>
      <form (ngSubmit)="submit()">
        <div><label>Email</label><input type="email" [(ngModel)]="email" name="email" required placeholder="you@example.com" /></div>
        <div><label>Password</label><input type="password" [(ngModel)]="password" name="password" required placeholder="••••••••" /></div>
        <button type="submit">Sign In</button>
        <p *ngIf="error()" class="error">{{error()}}</p>
      </form>
      <p>No account? <a routerLink="/register">Create one</a></p>
    </div>
  `
})
export class LoginComponent {
  email = ''; password = '';
  error = signal('');

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => this.router.navigate(['/tournaments']),
      error: (err) => this.error.set(err.error?.message ?? 'Login failed')
    });
  }
}
