import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface AdminUserItem {
  userId: string;
  email: string;
  role: string;
  isActive: boolean;
  displayName?: string;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private http: HttpClient) {}

  getUsers() {
    return this.http.get<AdminUserItem[]>(`${environment.apiUrl}/admin/users`);
  }

  setRole(userId: string, role: string) {
    return this.http.patch<void>(`${environment.apiUrl}/admin/users/${userId}/role`, { role });
  }

  suspend(userId: string) {
    return this.http.post<void>(`${environment.apiUrl}/admin/users/${userId}/suspend`, {});
  }

  reinstate(userId: string) {
    return this.http.post<void>(`${environment.apiUrl}/admin/users/${userId}/reinstate`, {});
  }
}

