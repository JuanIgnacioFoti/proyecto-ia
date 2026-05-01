import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TournamentService } from '../../services/tournament.service';
import { VideogameService } from '../../services/videogame.service';
import { Videogame } from '../../models';

@Component({
  selector: 'app-tournament-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }
    .create-card {
      max-width: 680px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      padding: 2rem 2.5rem;
      margin-top: 1.5rem;
      position: relative;
      overflow: hidden;
    }
    .create-card::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 3px;
      background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%);
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }
    .form-row-3 {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr;
      gap: 1rem;
    }
    @media (max-width: 540px) {
      .create-card { padding: 1.5rem 1rem; }
      .form-row, .form-row-3 { grid-template-columns: 1fr; }
    }
    .form-hint {
      font-size: 0.78rem;
      color: var(--color-text-faint);
      margin-top: -0.5rem;
    }
  `],
  template: `
    <div class="page">
      <h2>Create Tournament</h2>
      <div class="create-card">
        <form (ngSubmit)="submit()">
          <div><label>Tournament Name</label><input [(ngModel)]="name" name="name" required placeholder="e.g. Summer Championship 2026" /></div>
          <div><label>Description</label><textarea [(ngModel)]="description" name="description" placeholder="Describe your tournament..."></textarea></div>
          <div><label>Videogame</label>
            <select [(ngModel)]="videogameId" name="videogameId">
              @for (g of games(); track g.id) { <option [value]="g.id">{{g.name}}</option> }
            </select>
          </div>
          <div class="form-row">
            <div><label>Max Teams</label><input type="number" [(ngModel)]="maxTeams" name="maxTeams" required min="2" /></div>
            <div><label>Min Members Per Team</label><input type="number" [(ngModel)]="minMembersPerTeam" name="minMembersPerTeam" required min="1" /></div>
          </div>
          <div class="form-row">
            <div><label>Start Date</label><input type="datetime-local" [(ngModel)]="startDate" name="startDate" required /></div>
            <div><label>Estimated End Date</label><input type="datetime-local" [(ngModel)]="estimatedEndDate" name="estimatedEndDate" required /></div>
          </div>
          <div><label>Scoring System</label>
            <select [(ngModel)]="scoringType" name="scoringType">
              <option value="Standard">Standard (3 / 1 / 0)</option>
              <option value="WinnerTakesAll">Winner Takes All (3 / 0 / 0)</option>
              <option value="Custom">Custom</option>
            </select>
          </div>
          @if (scoringType === 'Custom') {
            <div class="form-row-3">
              <div><label>Win Points</label><input type="number" [(ngModel)]="customWin" name="customWin" required min="0" /></div>
              <div><label>Draw Points</label><input type="number" [(ngModel)]="customDraw" name="customDraw" required min="0" /></div>
              <div><label>Loss Points</label><input type="number" [(ngModel)]="customLoss" name="customLoss" required min="0" /></div>
            </div>
            <p class="form-hint">Win ≥ Draw ≥ Loss; all values must be non-negative integers.</p>
          }
          <p class="form-hint">Team size limits apply to roster registrations.</p>
          <button type="submit">🏆 Create Tournament</button>
          <p *ngIf="error()" class="error">{{error()}}</p>
        </form>
      </div>
    </div>
  `
})
export class TournamentCreateComponent implements OnInit {
  name = ''; description = ''; videogameId = '';
  maxTeams = 8; minMembersPerTeam = 5;
  startDate = ''; estimatedEndDate = '';
  scoringType = 'Standard';
  customWin = 3; customDraw = 1; customLoss = 0;
  games = signal<Videogame[]>([]);
  error = signal('');

  constructor(private ts: TournamentService, private vg: VideogameService, private router: Router) {}

  ngOnInit() {
    this.vg.getAll().subscribe(g => { this.games.set(g); if (g.length) this.videogameId = g[0].id; });
  }

  submit() {
    this.ts.create({
      name: this.name, description: this.description, videogameId: this.videogameId,
      maxTeams: this.maxTeams, minMembersPerTeam: this.minMembersPerTeam,
      startDate: new Date(this.startDate).toISOString(),
      estimatedEndDate: new Date(this.estimatedEndDate).toISOString(),
      scoringSystem: this.scoringType === 'Custom'
        ? { type: this.scoringType, winPoints: this.customWin, drawPoints: this.customDraw, lossPoints: this.customLoss }
        : { type: this.scoringType }
    }).subscribe({
      next: t => this.router.navigate(['/tournaments', t.id]),
      error: err => this.error.set(err.error?.message ?? 'Failed to create tournament')
    });
  }
}
