import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { VideogameService } from '../../services/videogame.service';
import { Videogame } from '../../models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  styles: [`
    :host { display: block; }
    .role-selector {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 1.25rem;
    }
    .role-btn {
      flex: 1;
      padding: 0.55rem 1rem;
      background: var(--color-surface-2);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      color: var(--color-text-muted);
      font-size: 0.875rem;
      font-weight: 600;
      font-family: var(--font-sans);
      cursor: pointer;
      transition: all 0.2s ease;
      text-align: center;
    }
    .role-btn:hover {
      border-color: var(--color-border-hover);
      color: var(--color-text);
    }
    .role-btn.active {
      background: rgba(99, 102, 241, 0.15);
      border-color: var(--color-primary);
      color: var(--color-primary-hover);
      box-shadow: 0 0 0 1px var(--color-primary);
    }
    .role-label {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--color-text-muted);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 0.3rem;
      display: block;
    }
    .auth-subtitle {
      text-align: center;
      color: var(--color-text-faint);
      font-size: 0.875rem;
      margin-top: -0.75rem;
      margin-bottom: 1.5rem;
    }
  `],
  template: `
    <div class="auth-container">
      <h2>Register</h2>
      <p class="auth-subtitle">Create your esports account</p>
      <div>
        <span class="role-label">I am a</span>
        <div class="role-selector">
          <button type="button" class="role-btn" [class.active]="role === 'player'" (click)="role = 'player'">🎮 Player</button>
          <button type="button" class="role-btn" [class.active]="role === 'organizer'" (click)="role = 'organizer'">🏆 Organizer</button>
        </div>
      </div>
      <form (ngSubmit)="submit()">
        <div><label>Email</label><input type="email" [(ngModel)]="email" name="email" required /></div>
        <div><label>Password</label><input type="password" [(ngModel)]="password" name="password" required /></div>
        @if (role === 'player') {
          <div><label>Username</label><input [(ngModel)]="username" name="username" required /></div>
          <div><label>Nombre Real</label><input [(ngModel)]="realName" name="realName" required /></div>
          <div><label>Videogame</label>
            <select [(ngModel)]="videogameId" name="videogameId">
              @for (g of games(); track g.id) {
                <option [value]="g.id">{{g.name}}</option>
              }
            </select>
          </div>
        }
        @if (role === 'organizer') {
          <div><label>Organization Name</label><input [(ngModel)]="organizationName" name="organizationName" required /></div>
        }
        <button type="submit">Register</button>
        <p *ngIf="error()" class="error">{{error()}}</p>
      </form>
      <p>Have an account? <a routerLink="/login">Login</a></p>
    </div>
  `
})
export class RegisterComponent implements OnInit {
  role = 'player'; email = ''; password = '';
  username = ''; realName = ''; videogameId = ''; organizationName = '';
  games = signal<Videogame[]>([]);
  error = signal('');

  constructor(private auth: AuthService, private vg: VideogameService, private router: Router) {}

  ngOnInit() {
    this.vg.getAll().subscribe(g => { this.games.set(g); if (g.length) this.videogameId = g[0].id; });
  }

  submit() {
    const obs = this.role === 'player'
      ? this.auth.registerPlayer({ email: this.email, password: this.password, username: this.username, realName: this.realName, videogameId: this.videogameId })
      : this.auth.registerOrganizer({ email: this.email, password: this.password, organizationName: this.organizationName });
    obs.subscribe({
      next: () => this.router.navigate(['/tournaments']),
      error: (err) => this.error.set(err.error?.message ?? 'Registration failed')
    });
  }
}
