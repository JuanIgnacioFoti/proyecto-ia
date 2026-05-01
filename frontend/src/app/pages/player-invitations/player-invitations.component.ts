import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TeamService } from '../../services/team.service';
import { Invitation } from '../../models';

@Component({
  selector: 'app-player-invitations',
  standalone: true,
  imports: [CommonModule],
  styles: [`
    :host { display: block; }
    .invitation-card {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
      flex-wrap: wrap;
      padding: 1rem 1.25rem;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      margin-bottom: 0.75rem;
      transition: border-color 0.2s ease;
    }
    .invitation-card:hover { border-color: var(--color-border-hover); }
    .inv-info { flex: 1; }
    .inv-team { font-weight: 700; font-size: 1rem; color: var(--color-text); margin-bottom: 0.2rem; }
    .inv-meta { font-size: 0.8rem; color: var(--color-text-faint); }
    .inv-actions { display: flex; gap: 0.5rem; }
    .btn-accept {
      background: rgba(16, 185, 129, 0.15);
      border: 1px solid rgba(16, 185, 129, 0.4);
      color: #34d399;
      padding: 0.4rem 1rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      font-family: var(--font-sans);
      font-weight: 600;
      font-size: 0.85rem;
      transition: all 0.2s;
    }
    .btn-accept:hover { background: rgba(16, 185, 129, 0.25); }
    .btn-decline {
      background: rgba(239, 68, 68, 0.1);
      border: 1px solid rgba(239, 68, 68, 0.3);
      color: #fca5a5;
      padding: 0.4rem 1rem;
      border-radius: var(--radius-md);
      cursor: pointer;
      font-family: var(--font-sans);
      font-weight: 600;
      font-size: 0.85rem;
      transition: all 0.2s;
    }
    .btn-decline:hover { background: rgba(239, 68, 68, 0.2); }
    .status-expired {
      font-size: 0.75rem;
      background: rgba(100, 116, 139, 0.2);
      color: #94a3b8;
      border: 1px solid rgba(100, 116, 139, 0.3);
      border-radius: 99px;
      padding: 0.2rem 0.6rem;
    }
    .empty-state {
      text-align: center;
      padding: 4rem 2rem;
      color: var(--color-text-faint);
    }
    .empty-icon { font-size: 2.5rem; display: block; margin-bottom: 1rem; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Team Invitations</h2>
      </div>
      @if (loading()) {
        <p>Loading invitations...</p>
      } @else if (invitations().length === 0) {
        <div class="empty-state">
          <span class="empty-icon">📬</span>
          <p>No pending invitations.</p>
        </div>
      } @else {
        @for (inv of invitations(); track inv.id) {
          <div class="invitation-card">
            <div class="inv-info">
              <div class="inv-team">{{ inv.teamName }}</div>
              <div class="inv-meta">Expires: {{ inv.expiresAt | date:'mediumDate' }}</div>
            </div>
            <div class="inv-actions">
              @if (isExpired(inv)) {
                <span class="status-expired">Expired</span>
              } @else if (inv.status === 'Pending') {
                <button class="btn-accept" (click)="respond(inv, true)">Accept</button>
                <button class="btn-decline" (click)="respond(inv, false)">Decline</button>
              } @else {
                <span class="status-expired">{{ inv.status }}</span>
              }
            </div>
          </div>
        }
      }
      @if (error()) { <p class="error">{{ error() }}</p> }
    </div>
  `
})
export class PlayerInvitationsComponent implements OnInit {
  invitations = signal<Invitation[]>([]);
  loading = signal(true);
  error = signal('');

  constructor(private teamService: TeamService) {}

  ngOnInit() {
    this.teamService.getMyInvitations().subscribe({
      next: inv => { this.invitations.set(inv); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('Failed to load invitations'); }
    });
  }

  isExpired(inv: Invitation): boolean {
    return new Date(inv.expiresAt) < new Date();
  }

  respond(inv: Invitation, accept: boolean) {
    this.teamService.respondToInvitation(inv.id, accept).subscribe({
      next: () => {
        this.invitations.update(list => list.filter(i => i.id !== inv.id));
      },
      error: err => this.error.set(err.error?.message ?? 'Failed to respond')
    });
  }
}
