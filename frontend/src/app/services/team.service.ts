import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { TeamDetail, Invitation } from '../models';

@Injectable({ providedIn: 'root' })
export class TeamService {
  constructor(private http: HttpClient) {}

  getMyTeam() {
    return this.http.get<TeamDetail>(`${environment.apiUrl}/teams/my`);
  }

  getById(id: string) {
    return this.http.get<TeamDetail>(`${environment.apiUrl}/teams/${id}`);
  }

  create(req: { name: string; videogameId: string; description?: string }) {
    return this.http.post<TeamDetail>(`${environment.apiUrl}/teams`, req);
  }

  invitePlayer(teamId: string, req: { username?: string; email?: string }) {
    return this.http.post<void>(`${environment.apiUrl}/teams/${teamId}/invitations`, req);
  }

  removeMember(teamId: string, memberId: string) {
    return this.http.delete<void>(`${environment.apiUrl}/teams/${teamId}/members/${memberId}`);
  }

  transferCaptaincy(teamId: string, newCaptainId: string) {
    return this.http.post<void>(`${environment.apiUrl}/teams/${teamId}/transfer-captaincy`, { newCaptainId });
  }

  getMyInvitations() {
    return this.http.get<Invitation[]>(`${environment.apiUrl}/teams/invitations/my`);
  }

  respondToInvitation(invitationId: string, accept: boolean) {
    return this.http.post<void>(`${environment.apiUrl}/teams/invitations/${invitationId}/respond`, { accept });
  }
}
