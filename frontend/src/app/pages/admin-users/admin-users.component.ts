import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, AdminUserItem } from '../../services/admin.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [`
    :host { display: block; }
    .user-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; margin-top: 0.5rem; }
    th { text-align: left; padding: 0.5rem 0.75rem; color: var(--color-text-faint); font-size: 0.75rem; font-weight: 600; text-transform: uppercase; border-bottom: 1px solid var(--color-border); }
    td { padding: 0.6rem 0.75rem; border-bottom: 1px solid rgba(99,102,241,0.08); color: var(--color-text-muted); vertical-align: middle; }
    tr:hover td { background: var(--color-surface-2); color: var(--color-text); }
    .badge-active { color: #6ee7b7; background: rgba(16,185,129,0.1); border: 1px solid rgba(16,185,129,0.3); border-radius: 4px; padding: 0.15rem 0.45rem; font-size: 0.75rem; }
    .badge-suspended { color: #fca5a5; background: rgba(239,68,68,0.1); border: 1px solid rgba(239,68,68,0.3); border-radius: 4px; padding: 0.15rem 0.45rem; font-size: 0.75rem; }
    .action-btn {
      padding: 0.3rem 0.65rem; border-radius: 4px; border: none; cursor: pointer;
      font-size: 0.78rem; font-weight: 600; margin-right: 0.35rem;
      font-family: var(--font-sans); transition: all 0.2s;
    }
    .btn-suspend { background: rgba(239,68,68,0.15); color: #fca5a5; border: 1px solid rgba(239,68,68,0.3); }
    .btn-suspend:hover { background: rgba(239,68,68,0.25); }
    .btn-reinstate { background: rgba(16,185,129,0.15); color: #6ee7b7; border: 1px solid rgba(16,185,129,0.3); }
    .btn-reinstate:hover { background: rgba(16,185,129,0.25); }
    .role-select { background: var(--color-surface-2); border: 1px solid var(--color-border); border-radius: 4px; color: var(--color-text); padding: 0.25rem 0.45rem; font-size: 0.8rem; }
    .toast { position: fixed; bottom: 1.5rem; right: 1.5rem; z-index: 9999; background: var(--color-surface-2); border: 1px solid var(--color-border); border-radius: var(--radius-md); padding: 0.65rem 1rem; color: var(--color-text); font-size: 0.875rem; box-shadow: 0 4px 16px rgba(0,0,0,0.4); }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>User Management</h2>
        <p class="page-subtitle">Manage all platform users</p>
      </div>

      @if (error()) {
        <p class="error">{{error()}}</p>
      }

      @if (users().length) {
        <table class="user-table">
          <thead>
            <tr>
              <th>Display Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (u of users(); track u.userId) {
              <tr>
                <td>{{u.displayName ?? '—'}}</td>
                <td>{{u.email}}</td>
                <td>
                  <select class="role-select" [ngModel]="u.role" (ngModelChange)="changeRole(u, $event)" [disabled]="loading()">
                    <option value="Player">Player</option>
                    <option value="Organizer">Organizer</option>
                    <option value="Admin">Admin</option>
                  </select>
                </td>
                <td>
                  @if (u.isActive) {
                    <span class="badge-active">Active</span>
                  } @else {
                    <span class="badge-suspended">Suspended</span>
                  }
                </td>
                <td>
                  @if (u.isActive) {
                    <button class="action-btn btn-suspend" (click)="suspendUser(u)" [disabled]="loading()">Suspend</button>
                  } @else {
                    <button class="action-btn btn-reinstate" (click)="reinstateUser(u)" [disabled]="loading()">Reinstate</button>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      } @else if (!error()) {
        <p style="color:var(--color-text-faint);font-size:0.875rem">No users found.</p>
      }
    </div>

    @if (toast()) {
      <div class="toast">{{toast()}}</div>
    }
  `
})
export class AdminUsersComponent implements OnInit {
  users = signal<AdminUserItem[]>([]);
  error = signal('');
  loading = signal(false);
  toast = signal('');
  private toastTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.adminService.getUsers().subscribe({
      next: users => this.users.set(users),
      error: () => this.error.set('Failed to load users.')
    });
  }

  changeRole(user: AdminUserItem, newRole: string) {
    if (newRole === user.role) return;
    this.loading.set(true);
    this.adminService.setRole(user.userId, newRole).subscribe({
      next: () => {
        this.users.update(list => list.map(u => u.userId === user.userId ? { ...u, role: newRole } : u));
        this.showToast(`Role updated to ${newRole}`);
        this.loading.set(false);
      },
      error: err => {
        this.showToast(err.error?.message ?? 'Failed to update role');
        this.loading.set(false);
      }
    });
  }

  suspendUser(user: AdminUserItem) {
    this.loading.set(true);
    this.adminService.suspend(user.userId).subscribe({
      next: () => {
        this.users.update(list => list.map(u => u.userId === user.userId ? { ...u, isActive: false } : u));
        this.showToast('User suspended');
        this.loading.set(false);
      },
      error: err => {
        this.showToast(err.error?.message ?? 'Failed to suspend user');
        this.loading.set(false);
      }
    });
  }

  reinstateUser(user: AdminUserItem) {
    this.loading.set(true);
    this.adminService.reinstate(user.userId).subscribe({
      next: () => {
        this.users.update(list => list.map(u => u.userId === user.userId ? { ...u, isActive: true } : u));
        this.showToast('User reinstated');
        this.loading.set(false);
      },
      error: err => {
        this.showToast(err.error?.message ?? 'Failed to reinstate user');
        this.loading.set(false);
      }
    });
  }

  private showToast(msg: string) {
    this.toast.set(msg);
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => this.toast.set(''), 3000);
  }
}
