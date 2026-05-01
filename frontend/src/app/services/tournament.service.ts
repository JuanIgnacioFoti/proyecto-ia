import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Match, Registration, Standing, TournamentDetail, TournamentSummary, CreateTournamentRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class TournamentService {
  constructor(private http: HttpClient) {}

  getPublicList() {
    return this.http.get<TournamentSummary[]>(`${environment.apiUrl}/tournaments`);
  }

  getMyTournaments() {
    return this.http.get<TournamentSummary[]>(`${environment.apiUrl}/tournaments/my`);
  }

  getById(id: string) {
    return this.http.get<TournamentDetail>(`${environment.apiUrl}/tournaments/${id}`);
  }

  create(req: CreateTournamentRequest) {
    return this.http.post<TournamentDetail>(`${environment.apiUrl}/tournaments`, req);
  }

  update(id: string, req: Partial<{ name: string; description: string; maxTeams: number; minMembersPerTeam: number; startDate: string; estimatedEndDate: string; }>) {
    return this.http.patch<TournamentDetail>(`${environment.apiUrl}/tournaments/${id}`, req);
  }

  advanceStatus(id: string) {
    return this.http.post<void>(`${environment.apiUrl}/tournaments/${id}/advance`, {});
  }

  suspend(id: string) {
    return this.http.post<void>(`${environment.apiUrl}/tournaments/${id}/suspend`, {});
  }

  reinstate(id: string) {
    return this.http.post<void>(`${environment.apiUrl}/tournaments/${id}/reinstate`, {});
  }

  delete(id: string) {
    return this.http.delete<void>(`${environment.apiUrl}/tournaments/${id}`);
  }

  getRegistrations(tournamentId: string) {
    return this.http.get<Registration[]>(`${environment.apiUrl}/tournaments/${tournamentId}/registrations`);
  }

  registerTeam(tournamentId: string, teamId: string) {
    return this.http.post<Registration>(`${environment.apiUrl}/tournaments/${tournamentId}/registrations`, { teamId });
  }

  withdrawTeam(tournamentId: string, teamId: string) {
    return this.http.delete<void>(`${environment.apiUrl}/tournaments/${tournamentId}/registrations/${teamId}`);
  }

  getMatches(tournamentId: string) {
    return this.http.get<Match[]>(`${environment.apiUrl}/tournaments/${tournamentId}/matches`);
  }

  recordMatch(tournamentId: string, req: { homeTeamId: string; awayTeamId: string; homeScore: number; awayScore: number; playedAt: string; }) {
    return this.http.post<Match>(`${environment.apiUrl}/tournaments/${tournamentId}/matches`, req);
  }

  updateMatch(tournamentId: string, matchId: string, req: { homeScore: number; awayScore: number; playedAt: string; }) {
    return this.http.put<Match>(`${environment.apiUrl}/tournaments/${tournamentId}/matches/${matchId}`, req);
  }

  getStandings(tournamentId: string) {
    return this.http.get<Standing[]>(`${environment.apiUrl}/tournaments/${tournamentId}/standings`);
  }
}
