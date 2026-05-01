export interface LoginRequest { email: string; password: string; }
export interface RegisterPlayerRequest { email: string; password: string; username: string; realName?: string; videogameId: string; }
export interface RegisterOrganizerRequest { email: string; password: string; organizationName: string; }
export interface AuthResult { token: string; userId: string; role: string; email: string; }

// Mirrors backend EsportsApp.Domain.Enums.TournamentStatus
export type TournamentStatus = 'Draft' | 'Open' | 'InProgress' | 'Completed' | 'Suspended';
export const STANDINGS_ALLOWED: TournamentStatus[] = ['InProgress', 'Completed'];

export interface UserProfile {
  userId: string; email: string; role: string;
  isActive: boolean;
  username?: string; realName?: string;
  mainVideogameId?: string; mainVideogameName?: string;
  organizationName?: string;
}

export interface Videogame { id: string; name: string; }

export interface TournamentSummary {
  id: string; name: string; description?: string; status: string;
  videogameId: string; videogameName: string;
  organizerName: string;
  startDate: string; estimatedEndDate: string;
  maxTeams: number; registeredTeams: number;
}

export interface ScoringSystem { type: string; winPoints: number; drawPoints: number; lossPoints: number; }

export interface TournamentDetail extends TournamentSummary {
  organizerId: string;
  minMembersPerTeam: number;
  scoringSystem: ScoringSystem;
  createdAt: string;
}

export interface CreateTournamentRequest {
  name: string; description?: string; videogameId: string; maxTeams: number;
  minMembersPerTeam: number;
  startDate: string; estimatedEndDate: string;
  scoringSystem: { type: string; winPoints?: number; drawPoints?: number; lossPoints?: number; };
}

export interface TeamDetail {
  id: string; name: string; description?: string; videogameId: string; videogameName: string;
  captainId: string; captainUsername: string; members: TeamMember[]; createdAt: string;
}

export interface TeamMember { playerId: string; username: string; realName?: string; isCaptain: boolean; joinedAt: string; }

export interface Invitation {
  id: string; teamId: string; teamName: string; videogameName: string;
  status: string; createdAt: string; expiresAt: string;
}

export interface Registration { id: string; tournamentId: string; teamId: string; teamName: string; registeredAt: string; status: string; }

export interface Match {
  id: string; tournamentId: string;
  homeTeamId: string; homeTeamName: string;
  awayTeamId: string; awayTeamName: string;
  homeScore: number; awayScore: number;
  playedAt: string; recordedAt: string;
}

export interface Standing {
  rank: number; teamId: string; teamName: string;
  points: number; matchesPlayed: number; wins: number; draws: number; losses: number;
}
