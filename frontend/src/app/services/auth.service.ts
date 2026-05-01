import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthResult, LoginRequest, RegisterOrganizerRequest, RegisterPlayerRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'esports_token';
  private readonly USER_KEY = 'esports_user';

  private _token = signal<string | null>(localStorage.getItem(this.TOKEN_KEY));
  private _user = signal<AuthResult | null>(JSON.parse(localStorage.getItem(this.USER_KEY) ?? 'null'));

  readonly token = this._token.asReadonly();
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => !!this._token());
  readonly role = computed(() => this._user()?.role ?? null);

  constructor(private http: HttpClient, private router: Router) {}

  registerPlayer(req: RegisterPlayerRequest) {
    return this.http.post<AuthResult>(`${environment.apiUrl}/auth/register/player`, req)
      .pipe(tap(r => this.store(r)));
  }

  registerOrganizer(req: RegisterOrganizerRequest) {
    return this.http.post<AuthResult>(`${environment.apiUrl}/auth/register/organizer`, req)
      .pipe(tap(r => this.store(r)));
  }

  login(req: LoginRequest) {
    return this.http.post<AuthResult>(`${environment.apiUrl}/auth/login`, req)
      .pipe(tap(r => this.store(r)));
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this._token.set(null);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  private store(result: AuthResult) {
    localStorage.setItem(this.TOKEN_KEY, result.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(result));
    this._token.set(result.token);
    this._user.set(result);
  }
}
