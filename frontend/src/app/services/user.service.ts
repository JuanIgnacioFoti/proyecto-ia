import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { UserProfile } from '../models';

export interface UpdatePlayerProfileRequest {
  realName: string;
  email?: string;
  currentPassword?: string;
  newPassword?: string;
}

export interface UpdateOrganizerProfileRequest {
  organizationName: string;
  currentPassword?: string;
  newPassword?: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(private http: HttpClient) {}

  getMe() {
    return this.http.get<UserProfile>(`${environment.apiUrl}/users/me`);
  }

  updatePlayerProfile(req: UpdatePlayerProfileRequest) {
    return this.http.patch<void>(`${environment.apiUrl}/users/me/player`, req);
  }

  updateOrganizerProfile(req: UpdateOrganizerProfileRequest) {
    return this.http.patch<void>(`${environment.apiUrl}/users/me/organizer`, req);
  }
}
