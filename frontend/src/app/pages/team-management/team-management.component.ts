import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TeamService } from '../../services/team.service';
import { VideogameService } from '../../services/videogame.service';
import { AuthService } from '../../services/auth.service';
import { TeamDetail, Videogame } from '../../models';

@Component({
  selector: 'app-team-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  styles: [`
    :host { display: block; }
    .team-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      padding: 2rem;
      margin-top: 1.5rem;
      position: relative;
      overflow: hidden;
    }
    .team-card::before {
      content: '';
      position: absolute;
      top: 0; left: 0; right: 0;
      height: 3px;
      background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%);
    }
    .create-card {
      max-width: 480px;
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
      top: 0; left: 0; right: 0;
      height: 3px;
      background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%);
    }
    .member-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.6rem 1rem;
      background: var(--color-surface-2);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      margin-bottom: 0.4rem;
    }
    .member-name { font-weight: 600; font-size: 0.9rem; }
    .captain-badge {
      font-size: 0.7rem;
      background: rgba(99, 102, 241, 0.2);
      color: var(--color-primary-hover);
      border: 1px solid rgba(99, 102, 241, 0.4);
      border-radius: 99px;
      padding: 0.15rem 0.6rem;
      font-weight: 700;
      margin-left: 0.5rem;
    }
    .btn-danger {
      background: rgba(239, 68, 68, 0.1);
      border: 1px solid rgba(239, 68, 68, 0.4);
      color: #fca5a5;
      font-size: 0.8rem;
      padding: 0.3rem 0.75rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      font-family: var(--font-sans);
      font-weight: 600;
      transition: all 0.2s;
    }
    .btn-danger:hover { background: rgba(239, 68, 68, 0.2); }
    .btn-outline {
      background: transparent;
      border: 1px solid var(--color-border-hover);
      color: var(--color-text-muted);
      font-size: 0.8rem;
      padding: 0.3rem 0.75rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      font-family: var(--font-sans);
      font-weight: 600;
      transition: all 0.2s;
    }
    .btn-outline:hover { border-color: var(--color-primary); color: var(--color-primary-hover); }
    .invite-row {
      display: flex;
      gap: 0.5rem;
      margin-top: 0.5rem;
    }
    .invite-row input { flex: 1; margin-bottom: 0; }
    .section-sub {
      font-size: 0.8rem;
      color: var(--color-text-faint);
      margin-bottom: 0.75rem;
    }
    .success {
      color: #6ee7b7 !important;
      background: rgba(16, 185, 129, 0.1);
      border: 1px solid rgba(16, 185, 129, 0.3);
      border-radius: var(--radius-md);
      padding: 0.5rem 1rem;
      font-size: 0.875rem;
      text-align: center;
    }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>My Team</h2>
        @if (team()) {
          <a routerLink="/invitations" class="btn">📬 Invitations</a>
        }
      </div>

      @if (!loaded()) {
        <p>Loading...</p>
      } @else if (!team()) {
        <!-- Create Team Form -->
        <div class="create-card">
          <h3 style="margin-top:0">Create a Team</h3>
          <form (ngSubmit)="createTeam()">
            <div>
              <label>Team Name</label>
              <input [(ngModel)]="newTeamName" name="teamName" required placeholder="e.g. Phoenix Rising" />
            </div>
            <div>
              <label>Description</label>
              <input [(ngModel)]="newTeamDescription" name="teamDescription" placeholder="Short description of your team" />
            </div>
            <div>
              <label>Videogame</label>
              <select [(ngModel)]="newTeamVgId" name="vgId">
                @for (g of games(); track g.id) {
                  <option [value]="g.id">{{ g.name }}</option>
                }
              </select>
            </div>
            <button type="submit">Create Team</button>
            @if (error()) { <p class="error">{{ error() }}</p> }
          </form>
        </div>
      } @else {
        <!-- Team Detail -->
        <div class="team-card">
          <div style="display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:1rem;">
            <div>
              <h3 style="margin-top:0;margin-bottom:0.25rem">{{ team()!.name }}</h3>
              <span style="font-size:0.85rem;color:var(--color-text-faint)">{{ team()!.videogameName }}</span>
            </div>
          </div>

          <h3>Roster ({{ team()!.members.length }})</h3>
          @for (m of team()!.members; track m.playerId) {
            <div class="member-row">
              <span class="member-name">
                {{ m.username }}
                @if (m.isCaptain) { <span class="captain-badge">Captain</span> }
              </span>
              @if (isCaptain() && !m.isCaptain) {
                <div style="display:flex;gap:0.5rem">
                  <button class="btn-outline" (click)="transferCaptaincy(m.playerId)">Make Captain</button>
                  <button class="btn-danger" (click)="removeMember(m.playerId)">Remove</button>
                </div>
              }
            </div>
          }
          @if (!team()!.members.length) {
            <p class="no-data" style="color:var(--color-text-faint);font-size:0.875rem;padding:0.5rem 0">No members yet.</p>
          }

          @if (isCaptain()) {
            <h3>Invite Player</h3>
            <p class="section-sub">Enter username or email of the player to invite.</p>
            <div class="invite-row">
              <input [(ngModel)]="inviteInput" placeholder="username or email" />
              <button type="button" class="btn" (click)="invitePlayer()">Invite</button>
            </div>
            @if (inviteSuccess()) { <p class="success" style="margin-top:0.5rem">{{ inviteSuccess() }}</p> }
            @if (inviteError()) { <p class="error" style="margin-top:0.5rem">{{ inviteError() }}</p> }
          }

          @if (error()) { <p class="error" style="margin-top:1rem">{{ error() }}</p> }
        </div>
      }
    </div>
  `
})
export class TeamManagementComponent implements OnInit {
  team = signal<TeamDetail | null>(null);
  games = signal<Videogame[]>([]);
  loaded = signal(false);

  newTeamName = '';
  newTeamVgId = '';
  newTeamDescription = '';
  inviteInput = '';
  error = signal('');
  inviteSuccess = signal('');
  inviteError = signal('');

  constructor(
    private teamService: TeamService,
    private vgService: VideogameService,
    public auth: AuthService
  ) {}

  ngOnInit() {
    this.vgService.getAll().subscribe(g => {
      this.games.set(g);
      if (g.length) this.newTeamVgId = g[0].id;
    });

    // Try to fetch the player's team via their profile team relationship
    // The API returns the team the current player belongs to (or 404)
    this.teamService.getMyTeam().subscribe({
      next: t => { this.team.set(t); this.loaded.set(true); },
      error: () => { this.team.set(null); this.loaded.set(true); }
    });
  }

  get isCaptain(): () => boolean {
    return () => {
      const t = this.team();
      const me = this.auth.user();
      return !!t && !!me && t.captainId === me.userId;
    };
  }

  createTeam() {
    this.error.set('');
    this.teamService.create({ name: this.newTeamName, videogameId: this.newTeamVgId, description: this.newTeamDescription || undefined }).subscribe({
      next: t => { this.team.set(t); },
      error: err => this.error.set(err.error?.message ?? 'Failed to create team')
    });
  }

  invitePlayer() {
    this.inviteSuccess.set('');
    this.inviteError.set('');
    const val = this.inviteInput.trim();
    if (!val) return;
    const req = val.includes('@') ? { email: val } : { username: val };
    this.teamService.invitePlayer(this.team()!.id, req).subscribe({
      next: () => { this.inviteSuccess.set('Invitation sent!'); this.inviteInput = ''; },
      error: err => this.inviteError.set(err.error?.message ?? 'Failed to send invite')
    });
  }

  removeMember(playerId: string) {
    this.teamService.removeMember(this.team()!.id, playerId).subscribe({
      next: () => this.refreshTeam(),
      error: err => this.error.set(err.error?.message ?? 'Failed to remove member')
    });
  }

  transferCaptaincy(newCaptainId: string) {
    this.teamService.transferCaptaincy(this.team()!.id, newCaptainId).subscribe({
      next: () => this.refreshTeam(),
      error: err => this.error.set(err.error?.message ?? 'Failed to transfer captaincy')
    });
  }

  private refreshTeam() {
    const t = this.team();
    if (!t) return;
    this.teamService.getById(t.id).subscribe(updated => this.team.set(updated));
  }
}
