import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Standing } from '../../models';

/**
 * Reusable standings table component (1224 standard competition ranking).
 * Renders an aria-live polite region so assistive technologies announce updates.
 */
@Component({
  selector: 'app-standings-table',
  standalone: true,
  imports: [CommonModule],
  styles: [`
    :host { display: block; }
    .no-data { color: var(--color-text-faint); font-size: 0.875rem; padding: 1rem 0; }
    table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
    th {
      text-align: left; padding: 0.5rem 0.75rem;
      color: var(--color-text-faint); font-size: 0.75rem; font-weight: 600;
      text-transform: uppercase; border-bottom: 1px solid var(--color-border);
    }
    td { padding: 0.55rem 0.75rem; border-bottom: 1px solid rgba(99,102,241,0.08); color: var(--color-text-muted); }
    tr:hover td { background: var(--color-surface-2); color: var(--color-text); }
    td:first-child { font-weight: 700; color: var(--color-text); }
    .rank-1 td:first-child { color: #fbbf24; }
    .rank-2 td:first-child { color: #94a3b8; }
    .rank-3 td:first-child { color: #f97316; }
  `],
  template: `
    <div aria-live="polite" aria-label="Tournament standings">
      @if (standings.length) {
        <table>
          <thead>
            <tr>
              <th scope="col">#</th>
              <th scope="col">Team</th>
              <th scope="col">Pts</th>
              <th scope="col">MP</th>
              <th scope="col">W</th>
              <th scope="col">D</th>
              <th scope="col">L</th>
            </tr>
          </thead>
          <tbody>
            @for (s of standings; track s.teamId) {
              <tr [class]="'rank-' + s.rank">
                <td>{{s.rank}}</td>
                <td>{{s.teamName}}</td>
                <td>{{s.points}}</td>
                <td>{{s.matchesPlayed}}</td>
                <td>{{s.wins}}</td>
                <td>{{s.draws}}</td>
                <td>{{s.losses}}</td>
              </tr>
            }
          </tbody>
        </table>
      } @else {
        <p class="no-data">Standings will appear once matches are played.</p>
      }
    </div>
  `
})
export class StandingsTableComponent {
  @Input() standings: Standing[] = [];
}
