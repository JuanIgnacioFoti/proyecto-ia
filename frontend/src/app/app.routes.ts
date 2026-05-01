import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'tournaments', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent) },
  { path: 'tournaments', loadComponent: () => import('./pages/tournament-list/tournament-list.component').then(m => m.TournamentListComponent) },
  { path: 'tournaments/create', canActivate: [roleGuard('Organizer')], loadComponent: () => import('./pages/tournament-create/tournament-create.component').then(m => m.TournamentCreateComponent) },
  { path: 'tournaments/:id/edit', canActivate: [roleGuard('Organizer')], loadComponent: () => import('./pages/tournament-edit/tournament-edit.component').then(m => m.TournamentEditComponent) },
  { path: 'tournaments/:id', loadComponent: () => import('./pages/tournament-detail/tournament-detail.component').then(m => m.TournamentDetailComponent) },
  { path: 'profile', canActivate: [authGuard], loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent) },
  { path: 'teams/my', canActivate: [roleGuard('Player')], loadComponent: () => import('./pages/team-management/team-management.component').then(m => m.TeamManagementComponent) },
  { path: 'invitations', canActivate: [roleGuard('Player')], loadComponent: () => import('./pages/player-invitations/player-invitations.component').then(m => m.PlayerInvitationsComponent) },
  { path: 'admin/users', canActivate: [roleGuard('Admin')], loadComponent: () => import('./pages/admin-users/admin-users.component').then(m => m.AdminUsersComponent) },
  { path: '**', redirectTo: 'tournaments' }
];

