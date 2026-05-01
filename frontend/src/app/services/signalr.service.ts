import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HttpTransportType, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

export type ConnectionStatus = 'connected' | 'disconnected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private connection: HubConnection | null = null;
  private standingsUpdated$ = new Subject<string>();
  private connectionStatus$ = new BehaviorSubject<ConnectionStatus>('disconnected');

  readonly standingsUpdated = this.standingsUpdated$.asObservable();
  /** Emits the current SignalR connection status for UI feedback. */
  readonly connectionStatus = this.connectionStatus$.asObservable();

  constructor(private auth: AuthService) {}

  async joinTournament(tournamentId: string): Promise<void> {
    if (!this.auth.isLoggedIn()) return;
    if (this.connection) await this.leaveAll();

    this.connection = new HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        // accessTokenFactory is called on every connect/reconnect — always fresh token
        accessTokenFactory: () => this.auth.token() ?? '',
        // Prefer WebSockets, fall back to LongPolling only if needed
        transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('StandingsUpdated', (id: string) => {
      this.standingsUpdated$.next(id);
    });

    this.connection.onreconnecting(() => {
      this.connectionStatus$.next('reconnecting');
    });

    this.connection.onreconnected(async () => {
      this.connectionStatus$.next('connected');
      // Re-join the tournament group after reconnection
      try {
        await this.connection!.invoke('JoinTournamentGroup', tournamentId);
      } catch (err) {
        console.error('[SignalR] Failed to re-join group after reconnect:', err);
      }
    });

    this.connection.onclose(() => {
      this.connectionStatus$.next('disconnected');
    });

    try {
      await this.connection.start();
      this.connectionStatus$.next('connected');
      await this.connection.invoke('JoinTournamentGroup', tournamentId);
    } catch (err) {
      console.error('[SignalR] Failed to connect:', err);
      this.connectionStatus$.next('disconnected');
      throw err;
    }
  }

  async leaveAll(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.connectionStatus$.next('disconnected');
    }
  }
}
