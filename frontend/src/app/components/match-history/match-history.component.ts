import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Match } from '../../models';

/**
 * Reusable match history component.
 * Shows a chronological list of recorded match results.
 */
@Component({
  selector: 'app-match-history',
  standalone: true,
  imports: [CommonModule],
  styles: [`
    :host { display: block; }
    .match-list { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.5rem; }
    .match-row {
      display: flex; align-items: center; justify-content: space-between; gap: 0.5rem;
      padding: 0.6rem 1rem;
      background: var(--color-surface); border: 1px solid var(--color-border);
      border-radius: var(--radius-md); font-size: 0.875rem; color: var(--color-text-muted);
      transition: border-color 0.2s ease, background 0.2s ease;
      cursor: default;
    }
    .match-row.clickable { cursor: pointer; }
    .match-row:hover { border-color: var(--color-border-hover); background: var(--color-surface-2); color: var(--color-text); }
    .match-score {
      font-weight: 700; font-size: 1rem; color: var(--color-text); min-width: 50px;
      text-align: center; background: var(--color-surface-2);
      border-radius: var(--radius-sm); padding: 0.1rem 0.5rem;
    }
    .match-team { flex: 1; }
    .match-team.home { text-align: right; }
    .match-team.away { text-align: left; }
    .no-data { color: var(--color-text-faint); font-size: 0.875rem; padding: 1rem 0; }
  `],
  template: `
    @if (matches.length) {
      <div class="match-list">
        @for (m of matches; track m.id) {
          <div class="match-row" [class.clickable]="!!onMatchClick"
               (click)="onMatchClick && onMatchClick(m)"
               (keydown.enter)="onMatchClick && onMatchClick(m)"
               [tabIndex]="onMatchClick ? 0 : -1"
               [attr.role]="onMatchClick ? 'button' : null"
               [attr.aria-label]="onMatchClick ? 'Edit: ' + m.homeTeamName + ' vs ' + m.awayTeamName : null">
            <span class="match-team home">{{m.homeTeamName}}</span>
            <span class="match-score">{{m.homeScore}} : {{m.awayScore}}</span>
            <span class="match-team away">{{m.awayTeamName}}</span>
          </div>
        }
      </div>
    } @else {
      <p class="no-data">Matches will be scheduled when the tournament starts.</p>
    }
  `
})
export class MatchHistoryComponent {
  @Input() matches: Match[] = [];
  /** Optional callback for organizer edit action. If provided, rows become interactive. */
  @Input() onMatchClick?: (match: Match) => void;
}
