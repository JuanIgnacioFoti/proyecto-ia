import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { TournamentService } from '../../services/tournament.service';
import { TeamService } from '../../services/team.service';
import { SignalRService } from '../../services/signalr.service';
import { AuthService } from '../../services/auth.service';
import { TournamentDetail, Match, Standing, Registration, TeamDetail, STANDINGS_ALLOWED } from '../../models';
import { ConnectionStatus } from '../../services/signalr.service';
import { StandingsTableComponent } from '../../components/standings-table/standings-table.component';
import { MatchHistoryComponent } from '../../components/match-history/match-history.component';

@Component({
  selector: 'app-tournament-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, StandingsTableComponent, MatchHistoryComponent],
  styles: [`
    :host { display: block; }
    .detail-hero {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
      flex-wrap: wrap;
      margin-bottom: 0.75rem;
    }
    .detail-hero h1 { margin-bottom: 0; flex: 1; font-size: 1.75rem; }
    .no-data { color: var(--color-text-faint); font-size: 0.875rem; padding: 1rem 0; }
    .advance-btn {
      background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
      box-shadow: 0 2px 8px rgba(245, 158, 11, 0.3);
    }
    .advance-btn:hover { background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%); }
    .delete-btn {
      background: rgba(239, 68, 68, 0.1);
      border: 1px solid rgba(239, 68, 68, 0.4);
      color: #fca5a5;
      padding: 0.55rem 1rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      font-family: var(--font-sans);
      font-weight: 600;
      font-size: 0.875rem;
      transition: all 0.2s;
    }
    .delete-btn:hover { background: rgba(239, 68, 68, 0.2); }
    .form-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      padding: 1.25rem 1.5rem;
      margin-top: 0.75rem;
    }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
    .form-row-3 { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.75rem; }
    .success {
      color: #6ee7b7 !important;
      background: rgba(16, 185, 129, 0.1);
      border: 1px solid rgba(16, 185, 129, 0.3);
      border-radius: var(--radius-md);
      padding: 0.4rem 0.75rem;
      font-size: 0.85rem;
    }
    .standings-updated {
      background: rgba(99, 102, 241, 0.15);
      border: 1px solid rgba(99, 102, 241, 0.4);
      border-radius: var(--radius-md);
      color: var(--color-primary-hover);
      padding: 0.4rem 0.75rem;
      font-size: 0.8rem;
      margin-bottom: 0.75rem;
    }
    .reg-row {
      display: flex; align-items: center; justify-content: space-between;
      padding: 0.5rem 0.75rem;
      background: var(--color-surface-2);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      margin-bottom: 0.35rem;
    }
    .connection-warning {
      background: rgba(245, 158, 11, 0.1);
      border: 1px solid rgba(245, 158, 11, 0.3);
      border-radius: var(--radius-md);
      color: #fbbf24;
      padding: 0.4rem 0.75rem;
      font-size: 0.8rem;
      margin-bottom: 0.75rem;
    }
  `],
  template: `
    <div class="page" *ngIf="tournament()">
      <!-- Hero -->
      <div class="detail-hero">
        <div>
          <h1>{{tournament()!.name}}</h1>
          <span style="font-size:0.85rem;color:var(--color-text-faint)">{{tournament()!.videogameName}} · by {{tournament()!.organizerName}}</span>
        </div>
        <span class="badge badge-{{tournament()!.status.toLowerCase()}}">{{tournament()!.status}}</span>
      </div>
      <p *ngIf="tournament()!.description">{{tournament()!.description}}</p>

      <!-- Organizer Actions -->
      @if (isOrganizer()) {
        <div class="actions">
          @if (tournament()!.status === 'Draft' || tournament()!.status === 'Open') {
            <a [routerLink]="['/tournaments', tournament()!.id, 'edit']" class="btn" style="background:var(--color-surface-2);border:1px solid var(--color-border-hover);color:var(--color-text-muted);box-shadow:none">✏ Edit</a>
          }
          @if (tournament()!.status !== 'Completed' && tournament()!.status !== 'Suspended') {
            <button class="advance-btn btn" (click)="advance()">&#9654; Advance Status</button>
          }
          @if (tournament()!.status === 'Draft') {
            <button class="delete-btn" (click)="deleteTournament()">🗑 Delete</button>
          }
        </div>
      }

      <!-- Registered Teams -->
      <h3>Registered Teams ({{registrations().length}} / {{tournament()!.maxTeams}})</h3>
      @if (registrations().length) {
        @for (r of registrations(); track r.id) {
          <div class="reg-row">
            <span>{{r.teamName}}</span>
            @if (isCaptain() && myTeam()?.id === r.teamId && tournament()!.status === 'Open') {
              <button class="delete-btn" style="font-size:0.75rem;padding:0.25rem 0.6rem" (click)="withdrawTeam(r.teamId)">Withdraw</button>
            }
          </div>
        }
      } @else {
        <p class="no-data">No teams registered yet.</p>
      }

      <!-- Captain: Register team -->
      @if (isCaptain() && tournament()!.status === 'Open' && !isAlreadyRegistered()) {
        <div style="margin-top:0.5rem">
          @if (teamBelowMinMembers()) {
            <p class="connection-warning" style="margin-bottom:0.5rem">
              ⚠ Your team needs at least {{ tournament()!.minMembersPerTeam }} member(s) to register (currently {{ myTeam()!.members.length }}).
            </p>
          }
          <button class="btn" [disabled]="teamBelowMinMembers()" (click)="registerMyTeam()">+ Register My Team</button>
          @if (regError()) { <p class="error" style="margin-top:0.5rem">{{regError()}}</p> }
          @if (regSuccess()) { <p class="success" style="margin-top:0.5rem">{{regSuccess()}}</p> }
        </div>
      }

      <!-- Standings -->
      <h3>Standings</h3>
      @if (connectionStatus() === 'reconnecting' || connectionStatus() === 'disconnected') {
        <p class="connection-warning" role="status" aria-live="polite">⚠ Live updates disconnected. Standings may be stale. Reconnecting...</p>
      }
      <p class="standings-updated" role="status" aria-live="polite" [style.display]="standingsFlash() ? 'block' : 'none'">✓ Standings updated</p>
      @if (!auth.isLoggedIn()) {
        <p class="no-data">Log in to view standings.</p>
      } @else if (standingsUnavailable()) {
        <p class="no-data">Standings are only available once the tournament is in progress or completed. Current status: <strong>{{tournament()!.status}}</strong>.</p>
      } @else {
        <app-standings-table [standings]="standings()" />
      }

      <!-- Matches -->
      <h3>Match History</h3>
      <app-match-history [matches]="matches()" [onMatchClick]="isOrganizer() ? selectMatchForEdit.bind(this) : undefined" />

      <!-- Organizer: Record/Edit Match -->
      @if (isOrganizer() && tournament()!.status === 'InProgress') {
        <h3>{{editingMatch() ? 'Edit Match Result' : 'Record Match Result'}}</h3>
        <div class="form-card">
          <form (ngSubmit)="submitMatch()">
            @if (!editingMatch()) {
              <div class="form-row">
                <div>
                  <label>Home Team</label>
                  <select [(ngModel)]="matchHomeTeamId" name="homeTeamId">
                    <option value="">Select team...</option>
                    @for (r of registrations(); track r.teamId) {
                      <option [value]="r.teamId">{{r.teamName}}</option>
                    }
                  </select>
                </div>
                <div>
                  <label>Away Team</label>
                  <select [(ngModel)]="matchAwayTeamId" name="awayTeamId">
                    <option value="">Select team...</option>
                    @for (r of registrations(); track r.teamId) {
                      <option [value]="r.teamId">{{r.teamName}}</option>
                    }
                  </select>
                </div>
              </div>
            } @else {
              <p style="color:var(--color-text-muted);font-size:0.875rem;margin-bottom:0.5rem">
                Editing: <strong>{{editingMatch()!.homeTeamName}} vs {{editingMatch()!.awayTeamName}}</strong>
              </p>
            }
            <div class="form-row-3">
              <div>
                <label>Home Score</label>
                <input type="number" [(ngModel)]="matchHomeScore" name="homeScore" min="0" />
              </div>
              <div>
                <label>Away Score</label>
                <input type="number" [(ngModel)]="matchAwayScore" name="awayScore" min="0" />
              </div>
              <div>
                <label>Date Played</label>
                <input type="datetime-local" [(ngModel)]="matchPlayedAt" name="playedAt" />
              </div>
            </div>
            <div class="actions" style="margin-top:0.75rem">
              <button type="submit">{{editingMatch() ? '✏ Update Result' : '+ Record Result'}}</button>
              @if (editingMatch()) {
                <button type="button" style="background:transparent;border:1px solid var(--color-border-hover);color:var(--color-text-muted)" (click)="cancelEdit()">Cancel</button>
              }
            </div>
            @if (matchError()) { <p class="error">{{matchError()}}</p> }
            @if (matchSuccess()) { <p class="success">{{matchSuccess()}}</p> }
          </form>
        </div>
      }
    </div>
  `
})
export class TournamentDetailComponent implements OnInit, OnDestroy {
  tournament = signal<TournamentDetail | null>(null);
  standings = signal<Standing[]>([]);
  matches = signal<Match[]>([]);
  registrations = signal<Registration[]>([]);
  myTeam = signal<TeamDetail | null>(null);

  editingMatch = signal<Match | null>(null);
  matchHomeTeamId = '';
  matchAwayTeamId = '';
  matchHomeScore = 0;
  matchAwayScore = 0;
  matchPlayedAt = '';
  matchError = signal('');
  matchSuccess = signal('');

  regError = signal('');
  regSuccess = signal('');
  connectionStatus = signal<ConnectionStatus>('disconnected');
  standingsFlash = signal(false);
  standingsUnavailable = signal(false);

  private sub!: Subscription;
  private connSub!: Subscription;
  private flashTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private route: ActivatedRoute,
    private ts: TournamentService,
    private teamService: TeamService,
    public auth: AuthService,
    private signalR: SignalRService,
    private meta: Meta,
    private titleService: Title
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.ts.getById(id).subscribe(t => {
      this.tournament.set(t);
      this.titleService.setTitle(`${t.name} — Esports Tournament Platform`);
      this.meta.updateTag({ name: 'description', content: `${t.name} — ${t.videogameName} tournament organized by ${t.organizerName}. Status: ${t.status}.` });
      if (this.auth.isLoggedIn()) {
        if (STANDINGS_ALLOWED.includes(t.status as any)) {
          this.ts.getStandings(id).subscribe({
            next: s => this.standings.set(s),
            error: () => this.standingsUnavailable.set(true)
          });
        } else {
          this.standingsUnavailable.set(true);
        }
      }
    });
    this.ts.getMatches(id).subscribe(m => this.matches.set(m));
    this.ts.getRegistrations(id).subscribe(r => this.registrations.set(r));
    if (this.auth.role() === 'Player') {
      this.teamService.getMyTeam().subscribe({ next: t => this.myTeam.set(t), error: () => {} });
    }

    this.signalR.joinTournament(id).catch(() => this.connectionStatus.set('disconnected'));

    if (!this.auth.isLoggedIn()) return;

    this.connSub = this.signalR.connectionStatus.subscribe(status => {
      this.connectionStatus.set(status);
    });

    this.sub = this.signalR.standingsUpdated.subscribe(() => {
      const t = this.tournament();
      if (!t || !STANDINGS_ALLOWED.includes(t.status as any)) return;
      this.ts.getStandings(id).subscribe({
        next: s => {
          this.standings.set(s);
          this.standingsUnavailable.set(false);
          this.flashStandings();
        },
        error: () => {}
      });
    });
  }

  isOrganizer(): boolean {
    const t = this.tournament();
    const me = this.auth.user();
    return !!t && !!me && t.organizerId === me.userId;
  }

  isCaptain(): boolean {
    return !!this.myTeam() && this.auth.role() === 'Player';
  }

  isAlreadyRegistered(): boolean {
    const myId = this.myTeam()?.id;
    return !!myId && this.registrations().some(r => r.teamId === myId && r.status === 'Active');
  }

  teamBelowMinMembers(): boolean {
    const t = this.tournament();
    const team = this.myTeam();
    if (!t || !team) return false;
    return team.members.length < t.minMembersPerTeam;
  }

  advance() {
    const t = this.tournament();
    if (!t) return;
    this.ts.advanceStatus(t.id).subscribe(() => {
      this.ts.getById(t.id).subscribe(u => this.tournament.set(u));
    });
  }

  deleteTournament() {
    const t = this.tournament();
    if (!t || !confirm('Delete this tournament?')) return;
    this.ts.delete(t.id).subscribe(() => {
      window.history.back();
    });
  }

  registerMyTeam() {
    this.regError.set('');
    this.regSuccess.set('');
    const t = this.tournament();
    const team = this.myTeam();
    if (!t || !team) return;
    this.ts.registerTeam(t.id, team.id).subscribe({
      next: () => {
        this.regSuccess.set('Team registered!');
        this.ts.getRegistrations(t.id).subscribe(r => this.registrations.set(r));
      },
      error: err => this.regError.set(err.error?.message ?? 'Registration failed')
    });
  }

  withdrawTeam(teamId: string) {
    const t = this.tournament();
    if (!t) return;
    this.ts.withdrawTeam(t.id, teamId).subscribe({
      next: () => this.ts.getRegistrations(t.id).subscribe(r => this.registrations.set(r)),
      error: err => this.regError.set(err.error?.message ?? 'Withdrawal failed')
    });
  }

  selectMatchForEdit(m: Match) {
    if (!this.isOrganizer()) return;
    this.editingMatch.set(m);
    this.matchHomeScore = m.homeScore;
    this.matchAwayScore = m.awayScore;
    this.matchPlayedAt = m.playedAt?.substring(0, 16) ?? '';
    this.matchError.set('');
    this.matchSuccess.set('');
  }

  cancelEdit() {
    this.editingMatch.set(null);
    this.matchHomeTeamId = '';
    this.matchAwayTeamId = '';
    this.matchHomeScore = 0;
    this.matchAwayScore = 0;
    this.matchPlayedAt = '';
  }

  submitMatch() {
    this.matchError.set('');
    this.matchSuccess.set('');
    const t = this.tournament();
    if (!t) return;
    const editing = this.editingMatch();

    if (editing) {
      this.ts.updateMatch(t.id, editing.id, {
        homeScore: this.matchHomeScore,
        awayScore: this.matchAwayScore,
        playedAt: new Date(this.matchPlayedAt).toISOString()
      }).subscribe({
        next: () => {
          this.matchSuccess.set('Match updated.');
          this.editingMatch.set(null);
          this.ts.getMatches(t.id).subscribe(m => this.matches.set(m));
        },
        error: err => this.matchError.set(err.error?.message ?? 'Update failed')
      });
    } else {
      if (!this.matchHomeTeamId || !this.matchAwayTeamId) {
        this.matchError.set('Please select both teams.'); return;
      }
      if (this.matchHomeTeamId === this.matchAwayTeamId) {
        this.matchError.set('Home and away teams must be different.'); return;
      }
      this.ts.recordMatch(t.id, {
        homeTeamId: this.matchHomeTeamId,
        awayTeamId: this.matchAwayTeamId,
        homeScore: this.matchHomeScore,
        awayScore: this.matchAwayScore,
        playedAt: new Date(this.matchPlayedAt).toISOString()
      }).subscribe({
        next: () => {
          this.matchSuccess.set('Match recorded!');
          this.matchHomeTeamId = ''; this.matchAwayTeamId = '';
          this.matchHomeScore = 0; this.matchAwayScore = 0; this.matchPlayedAt = '';
          this.ts.getMatches(t.id).subscribe(m => this.matches.set(m));
        },
        error: err => this.matchError.set(err.error?.message ?? 'Failed to record match')
      });
    }
  }

  private flashStandings() {
    this.standingsFlash.set(true);
    if (this.flashTimer) clearTimeout(this.flashTimer);
    this.flashTimer = setTimeout(() => this.standingsFlash.set(false), 3000);
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
    this.connSub?.unsubscribe();
    if (this.flashTimer) clearTimeout(this.flashTimer);
    this.signalR.leaveAll();
  }
}