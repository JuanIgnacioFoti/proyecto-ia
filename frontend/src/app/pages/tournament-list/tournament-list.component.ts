import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { TournamentService } from '../../services/tournament.service';
import { TournamentSummary } from '../../models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-tournament-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  styles: [`
    :host { display: block; }
    .empty-state {
      text-align: center;
      padding: 4rem 2rem;
      color: var(--color-text-faint);
    }
    .empty-state .empty-icon {
      font-size: 3rem;
      margin-bottom: 1rem;
      display: block;
    }
    .empty-state p {
      color: var(--color-text-faint);
      font-size: 0.95rem;
    }
    .loading-skeleton {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.5rem;
      margin-top: 1.5rem;
    }
    .skeleton-card {
      height: 160px;
      background: linear-gradient(90deg, var(--color-surface) 25%, var(--color-surface-2) 50%, var(--color-surface) 75%);
      background-size: 200% 100%;
      animation: shimmer 1.6s infinite;
      border-radius: var(--radius-lg);
      border: 1px solid var(--color-border);
    }
    @keyframes shimmer {
      0%   { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
    .card-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: auto;
    }
    .teams-count {
      font-size: 0.78rem;
      color: var(--color-text-faint);
    }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h1>Tournaments</h1>
        @if (auth.role() === 'Organizer') {
          <a routerLink="/tournaments/create" class="btn">Create Tournament</a>
        }
      </div>
      @if (loading()) {
        <div class="loading-skeleton">
          @for (i of [1,2,3,4,5,6]; track i) { <div class="skeleton-card"></div> }
        </div>
      } @else {
        @if (tournaments().length === 0) {
          <div class="empty-state">
            <span class="empty-icon">🏆</span>
            <p>No tournaments available yet.</p>
          </div>
        } @else {
          <div class="tournament-grid">
            @for (t of tournaments(); track t.id) {
              <a [routerLink]="['/tournaments', t.id]" class="tournament-card">
                <h3>{{t.name}}</h3>
                <p>{{t.videogameName}}</p>
                <div class="card-footer">
                  <span class="badge badge-{{t.status.toLowerCase()}}">{{t.status}}</span>
                  <span class="teams-count">👥 {{t.registeredTeams}} / {{t.maxTeams}}</span>
                </div>
              </a>
            }
          </div>
        }
      }
    </div>
  `
})
export class TournamentListComponent implements OnInit {
  tournaments = signal<TournamentSummary[]>([]);
  loading = signal(true);

  constructor(public auth: AuthService, private ts: TournamentService, private meta: Meta, private title: Title) {}

  ngOnInit() {
    this.title.setTitle('Tournaments — Esports Tournament Platform');
    this.meta.updateTag({ name: 'description', content: 'Browse all active esports tournaments. Find open competitions to join or completed results to review.' });
    this.ts.getPublicList().subscribe({ next: t => { this.tournaments.set(t); this.loading.set(false); }, error: () => this.loading.set(false) });
  }
}
