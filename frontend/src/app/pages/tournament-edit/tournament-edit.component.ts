import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TournamentService } from '../../services/tournament.service';
import { TournamentDetail } from '../../models';

@Component({
  selector: 'app-tournament-edit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }
    .edit-card {
      max-width: 680px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      padding: 2rem 2.5rem;
      margin-top: 1.5rem;
      position: relative;
      overflow: hidden;
    }
    .edit-card::before {
      content: '';
      position: absolute;
      top: 0; left: 0; right: 0;
      height: 3px;
      background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%);
    }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    @media (max-width: 540px) {
      .edit-card { padding: 1.5rem 1rem; }
      .form-row { grid-template-columns: 1fr; }
    }
    .form-hint { font-size: 0.78rem; color: var(--color-text-faint); margin-top: -0.5rem; }
    .cancel-btn {
      background: transparent;
      border: 1px solid var(--color-border-hover);
      color: var(--color-text-muted);
      padding: 0.625rem 1.5rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      font-family: var(--font-sans);
      font-weight: 600;
      font-size: 0.9rem;
      transition: all 0.2s;
    }
    .cancel-btn:hover { background: rgba(99,102,241,0.08); color: var(--color-text); }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h1>Edit Tournament</h1>
      </div>
      @if (loading()) {
        <p style="color:var(--color-text-faint)">Loading tournament...</p>
      } @else if (tournament()) {
        <div class="edit-card">
          <form (ngSubmit)="submit()">
            <div>
              <label for="name">Tournament Name</label>
              <input id="name" [(ngModel)]="name" name="name" required placeholder="e.g. Summer Championship 2026" />
            </div>
            <div>
              <label for="description">Description</label>
              <textarea id="description" [(ngModel)]="description" name="description" placeholder="Describe your tournament..."></textarea>
            </div>
            <div class="form-row">
              <div>
                <label for="maxTeams">Max Teams</label>
                <input id="maxTeams" type="number" [(ngModel)]="maxTeams" name="maxTeams" required min="2" />
              </div>
              <div>
                <label for="minMembers">Min Members Per Team</label>
                <input id="minMembers" type="number" [(ngModel)]="minMembersPerTeam" name="minMembersPerTeam" required min="1" />
              </div>
            </div>
            <div class="form-row">
              <div>
                <label for="startDate">Start Date</label>
                <input id="startDate" type="datetime-local" [(ngModel)]="startDate" name="startDate" required />
              </div>
              <div>
                <label for="endDate">Estimated End Date</label>
                <input id="endDate" type="datetime-local" [(ngModel)]="estimatedEndDate" name="estimatedEndDate" required />
              </div>
            </div>
            <p class="form-hint">Videogame and scoring system cannot be changed after creation.</p>
            <div class="actions">
              <button type="submit">✏ Save Changes</button>
              <button type="button" class="cancel-btn" (click)="cancel()">Cancel</button>
            </div>
            @if (error()) { <p class="error">{{error()}}</p> }
            @if (success()) { <p style="color:#6ee7b7;font-size:0.875rem">{{success()}}</p> }
          </form>
        </div>
      } @else {
        <p class="error">Tournament not found or you are not authorized to edit it.</p>
      }
    </div>
  `
})
export class TournamentEditComponent implements OnInit {
  tournament = signal<TournamentDetail | null>(null);
  loading = signal(true);
  error = signal('');
  success = signal('');

  name = '';
  description = '';
  maxTeams = 8;
  minMembersPerTeam = 5;
  startDate = '';
  estimatedEndDate = '';

  private tournamentId = '';

  constructor(
    private route: ActivatedRoute,
    private ts: TournamentService,
    private router: Router
  ) {}

  ngOnInit() {
    this.tournamentId = this.route.snapshot.params['id'];
    this.ts.getById(this.tournamentId).subscribe({
      next: t => {
        this.tournament.set(t);
        this.name = t.name;
        this.description = t.description ?? '';
        this.maxTeams = t.maxTeams;
        this.minMembersPerTeam = t.minMembersPerTeam;
        this.startDate = this.toLocalDatetime(t.startDate);
        this.estimatedEndDate = this.toLocalDatetime(t.estimatedEndDate);
        this.loading.set(false);
      },
      error: () => { this.tournament.set(null); this.loading.set(false); }
    });
  }

  submit() {
    this.error.set('');
    this.success.set('');
    this.ts.update(this.tournamentId, {
      name: this.name,
      description: this.description,
      maxTeams: this.maxTeams,
      minMembersPerTeam: this.minMembersPerTeam,
      startDate: new Date(this.startDate).toISOString(),
      estimatedEndDate: new Date(this.estimatedEndDate).toISOString()
    }).subscribe({
      next: () => {
        this.success.set('Tournament updated successfully!');
        setTimeout(() => this.router.navigate(['/tournaments', this.tournamentId]), 1200);
      },
      error: err => this.error.set(err.error?.message ?? 'Failed to update tournament')
    });
  }

  cancel() {
    this.router.navigate(['/tournaments', this.tournamentId]);
  }

  private toLocalDatetime(iso: string): string {
    if (!iso) return '';
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }
}
