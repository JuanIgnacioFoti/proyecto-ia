import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { ToastComponent } from './components/toast/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule, ToastComponent],
  styleUrl: './app.css',
  template: `
    <nav class="navbar">
      <a routerLink="/tournaments" class="brand">Esports Platform</a>
      <div class="nav-links">
        <a routerLink="/tournaments">Tournaments</a>
        @if (auth.isLoggedIn()) {
          @if (auth.role() === 'Player') {
            <a routerLink="/teams/my">My Team</a>
            <a routerLink="/invitations">Invitations</a>
          }
          @if (auth.role() === 'Organizer') {
            <a routerLink="/tournaments/create">+ New Tournament</a>
          }
          @if (auth.role() === 'Admin') {
            <a routerLink="/admin/users">Users</a>
          }
          <a routerLink="/profile">Profile</a>
          <span class="nav-user">{{auth.user()?.email}}</span>
          <button class="btn btn-sm" (click)="auth.logout()">Logout</button>
        } @else {
          <a routerLink="/login">Login</a>
          <a routerLink="/register">Register</a>
        }
      </div>
    </nav>
    <main><router-outlet /></main>
    <app-toast />
  `
})
export class App {
  constructor(public auth: AuthService) {}
}

