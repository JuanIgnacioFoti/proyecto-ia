import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { UserProfile } from '../../models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }
    .profile-card {
      max-width: 560px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      padding: 2rem 2.5rem;
      margin-top: 1.5rem;
      position: relative;
      overflow: hidden;
    }
    .profile-card::before {
      content: '';
      position: absolute;
      top: 0; left: 0; right: 0;
      height: 3px;
      background: linear-gradient(90deg, var(--color-primary) 0%, var(--color-accent) 100%);
    }
    .field-readonly {
      padding: 0.65rem 1rem;
      background: var(--color-surface-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      color: var(--color-text-faint);
      font-size: 0.95rem;
    }
    .success {
      color: #6ee7b7 !important;
      background: rgba(16, 185, 129, 0.1);
      border: 1px solid rgba(16, 185, 129, 0.3);
      border-radius: var(--radius-md);
      padding: 0.5rem 1rem;
      font-size: 0.875rem;
      text-align: center;
      margin-top: 0.5rem;
    }
    .hint {
      font-size: 0.75rem;
      color: var(--color-text-faint);
      margin-top: 0.2rem;
    }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>My Profile</h2>
        @if (profile()) {
          <span class="badge badge-{{ profile()!.role.toLowerCase() }}">{{ profile()!.role }}</span>
        }
      </div>
      @if (!profile()) {
        <p>Loading profile...</p>
      } @else {
        <div class="profile-card">
          <form (ngSubmit)="save()">
            @if (profile()!.role === 'Player') {
              <div>
                <label>Username</label>
                <div class="field-readonly">{{ profile()!.username }}</div>
                <span class="hint">Username cannot be changed.</span>
              </div>
              <div>
                <label>Real Name</label>
                <input [(ngModel)]="realName" name="realName" placeholder="Your full name" />
              </div>
              <div>
                <label>Email</label>
                <input type="email" [(ngModel)]="email" name="email" placeholder="Email address" />
              </div>
              <div>
                <label>Current Password</label>
                <input type="password" [(ngModel)]="currentPassword" name="currentPassword" placeholder="Required to change password" autocomplete="current-password" />
              </div>
              <div>
                <label>New Password</label>
                <input type="password" [(ngModel)]="newPassword" name="newPassword" placeholder="Leave blank to keep current" autocomplete="new-password" />
                <span class="hint">Min 8 chars, must contain uppercase, lowercase, and digit.</span>
              </div>
            }

            @if (profile()!.role === 'Organizer') {
              <div>
                <label>Email</label>
                <div class="field-readonly">{{ profile()!.email }}</div>
                <span class="hint">Email cannot be changed for Organizers.</span>
              </div>
              <div>
                <label>Organization Name</label>
                <input [(ngModel)]="organizationName" name="organizationName" required placeholder="Your organization name" />
              </div>
              <div>
                <label>Current Password</label>
                <input type="password" [(ngModel)]="orgCurrentPassword" name="orgCurrentPassword" placeholder="Required to change password" autocomplete="current-password" />
              </div>
              <div>
                <label>New Password</label>
                <input type="password" [(ngModel)]="orgNewPassword" name="orgNewPassword" placeholder="Leave blank to keep current" autocomplete="new-password" />
                <span class="hint">Min 8 chars, must contain uppercase, lowercase, and digit.</span>
              </div>
            }

            <button type="submit">Save Changes</button>
            @if (success()) { <p class="success">{{ success() }}</p> }
            @if (error()) { <p class="error">{{ error() }}</p> }
          </form>
        </div>
      }
    </div>
  `
})
export class ProfileComponent implements OnInit {
  profile = signal<UserProfile | null>(null);
  realName = '';
  email = '';
  currentPassword = '';
  newPassword = '';
  organizationName = '';
  orgCurrentPassword = '';
  orgNewPassword = '';
  success = signal('');
  error = signal('');

  constructor(private userService: UserService, public auth: AuthService) {}

  ngOnInit() {
    this.userService.getMe().subscribe({
      next: p => {
        this.profile.set(p);
        this.realName = p.realName ?? '';
        this.email = p.email ?? '';
        this.organizationName = p.organizationName ?? '';
      },
      error: () => this.error.set('Failed to load profile')
    });
  }

  save() {
    this.success.set('');
    this.error.set('');
    const p = this.profile();
    if (!p) return;

    if (p.role === 'Player') {
      this.userService.updatePlayerProfile({
        realName: this.realName,
        email: this.email !== p.email ? this.email : undefined,
        currentPassword: this.currentPassword || undefined,
        newPassword: this.newPassword || undefined
      }).subscribe({
        next: () => {
          this.profile.update(prev => prev ? { ...prev, realName: this.realName, email: this.email } : prev);
          this.currentPassword = '';
          this.newPassword = '';
          this.success.set('Profile updated successfully.');
        },
        error: err => this.error.set(err.error?.message ?? 'Update failed')
      });
    } else if (p.role === 'Organizer') {
      this.userService.updateOrganizerProfile({
        organizationName: this.organizationName,
        currentPassword: this.orgCurrentPassword || undefined,
        newPassword: this.orgNewPassword || undefined
      }).subscribe({
        next: () => {
          this.profile.update(prev => prev ? { ...prev, organizationName: this.organizationName } : prev);
          this.orgCurrentPassword = '';
          this.orgNewPassword = '';
          this.success.set('Profile updated successfully.');
        },
        error: err => this.error.set(err.error?.message ?? 'Update failed')
      });
    }
  }
}
