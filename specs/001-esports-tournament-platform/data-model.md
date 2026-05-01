# Data Model: Esports Tournament Management Platform

**Phase**: 1 | **Date**: 2026-04-20 | **Spec**: [spec.md](spec.md)

---

## Entity Relationship Overview

```
Videogame ──────────────────────────< Tournament (videogameId)
                                      Tournament ──< TournamentRegistration (tournamentId)
                                      Tournament ──< Match (tournamentId)
                                      Tournament ──< Standing (tournamentId)
                                      Tournament ──1 ScoringSystem

User ──< Player (userId)
         Player ──< Team (captainId)
         Player ──< TeamMember (playerId)
         Player ──< TeamInvitation (invitedPlayerId)

User ──< Organizer (userId)
         Organizer ──< Tournament (organizerId)

Team ──< TeamMember (teamId)
Team ──< TeamInvitation (teamId)
Team ──< TournamentRegistration (teamId)
Team ──< Match as HomeTeam (homeTeamId)
Team ──< Match as AwayTeam (awayTeamId)
Team ──< Standing (teamId)
```

---

## Entities

### User

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| Email | `string` | UNIQUE, NOT NULL, valid email format |
| PasswordHash | `string` | NOT NULL (BCrypt) |
| Role | `UserRole` | NOT NULL |
| CreatedAt | `DateTime` | NOT NULL, UTC |
| IsActive | `bool` | NOT NULL, default `true`; `false` = suspended |

**Notes**: `Role` encodes the top-level registration type (Player / Organizer / Admin). Team Captain status is derived from `Team.CaptainId`; it is not a column on `User`.

---

### Player

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK, FK → `User.Id` (1:1) |
| Username | `string` | UNIQUE, NOT NULL, **immutable after creation** |
| RealName | `string` | NOT NULL, updatable |
| MainVideogameId | `Guid` | FK → `Videogame.Id`, NOT NULL, **immutable after creation** |

---

### Organizer

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK, FK → `User.Id` (1:1) |
| OrganizationName | `string` | UNIQUE, NOT NULL, 3–60 characters, updatable |

---

### Videogame

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| Name | `string` | UNIQUE, NOT NULL |

**Notes**: Seeded at startup; **immutable at runtime** — no API endpoint to add/edit/remove.

Seed list (configurable in migration):
- League of Legends
- Valorant
- CS2
- Dota 2
- Rocket League

---

### Team

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| Name | `string` | NOT NULL, unique within `VideogameId` |
| Description | `string?` | nullable |
| VideogameId | `Guid` | FK → `Videogame.Id`, NOT NULL |
| CaptainId | `Guid` | FK → `Player.Id`, NOT NULL |
| CreatedAt | `DateTime` | NOT NULL, UTC |

**DB Constraints**:
- `UNIQUE(Name, VideogameId)`

**Service-Layer Constraints**:
- A `Player` cannot be Captain of more than one team per `VideogameId` (FR-016).
- `CaptainId` must correspond to an active `TeamMember` row at all times.

---

### TeamMember

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| TeamId | `Guid` | FK → `Team.Id`, NOT NULL |
| PlayerId | `Guid` | FK → `Player.Id`, NOT NULL |
| JoinedAt | `DateTime` | NOT NULL, UTC |

**DB Constraints**:
- `UNIQUE(TeamId, PlayerId)`

**Service-Layer Constraints**:
- A `Player` may appear in at most one active `TeamMember` row per `Videogame` (FR-019).

---

### TeamInvitation

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| TeamId | `Guid` | FK → `Team.Id`, NOT NULL |
| InvitedPlayerId | `Guid` | FK → `Player.Id`, NOT NULL |
| Status | `InvitationStatus` | NOT NULL, default `Pending` |
| CreatedAt | `DateTime` | NOT NULL, UTC |

**Expiry rule**: When `Status = Pending` and `CreatedAt + 7 days < UtcNow`, the invitation is treated as `Expired` at evaluation time (lazy — no background job required).

---

### Tournament

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| Name | `string` | NOT NULL |
| Description | `string?` | nullable |
| VideogameId | `Guid` | FK → `Videogame.Id`, NOT NULL |
| OrganizerId | `Guid` | FK → `Organizer.Id`, NOT NULL |
| StartDate | `DateTime` | NOT NULL, must be > `UtcNow` at creation time |
| EstimatedEndDate | `DateTime` | NOT NULL, must be > `StartDate` |
| MaxTeams | `int` | NOT NULL, ≥ 2 |
| MinMembersPerTeam | `int` | NOT NULL, ≥ 1, default `5` |
| Status | `TournamentStatus` | NOT NULL, default `Draft` |
| PreSuspensionStatus | `TournamentStatus?` | nullable — stores pre-`Suspended` state for reinstatement |
| CreatedAt | `DateTime` | NOT NULL, UTC |

---

### ScoringSystem

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| TournamentId | `Guid` | FK → `Tournament.Id`, NOT NULL, **UNIQUE** (1:1) |
| Type | `ScoringSystemType` | NOT NULL |
| WinPoints | `int` | NOT NULL, ≥ 0 |
| DrawPoints | `int` | NOT NULL, ≥ 0 |
| LossPoints | `int` | NOT NULL, ≥ 0 |

**Validation** (service layer, enforced at creation):
- `WinPoints ≥ DrawPoints ≥ LossPoints ≥ 0` (FR-030, required when `Type = Custom`).
- Standard: `WinPoints = 3, DrawPoints = 1, LossPoints = 0` (set by system).
- WinnerTakesAll: `WinPoints = 3, DrawPoints = 0, LossPoints = 0` (set by system).
- Immutable once tournament `Status` advances to `InProgress` (FR-032).

---

### TournamentRegistration

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| TournamentId | `Guid` | FK → `Tournament.Id`, NOT NULL |
| TeamId | `Guid` | FK → `Team.Id`, NOT NULL |
| RegisteredAt | `DateTime` | NOT NULL, UTC |
| Status | `RegistrationStatus` | NOT NULL, default `Active` |

**DB Constraints**:
- `UNIQUE(TournamentId, TeamId)`

---

### Match

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| TournamentId | `Guid` | FK → `Tournament.Id`, NOT NULL |
| HomeTeamId | `Guid` | FK → `Team.Id`, NOT NULL |
| AwayTeamId | `Guid` | FK → `Team.Id`, NOT NULL |
| HomeScore | `int` | NOT NULL, ≥ 0 |
| AwayScore | `int` | NOT NULL, ≥ 0 |
| PlayedAt | `DateTime` | NOT NULL, UTC |
| RecordedAt | `DateTime` | NOT NULL, UTC |

**DB Constraints**:
- `HomeTeamId ≠ AwayTeamId` (CHECK constraint)

**Service-Layer Constraints**:
- Duplicate detection (FR-035): reject if a match already exists for the same `TournamentId` and the same two teams (regardless of home/away order) at the same `PlayedAt`.

---

### Standing

| Field | Type | Constraints |
|-------|------|-------------|
| Id | `Guid` | PK |
| TournamentId | `Guid` | FK → `Tournament.Id`, NOT NULL |
| TeamId | `Guid` | FK → `Team.Id`, NOT NULL |
| Points | `int` | NOT NULL, ≥ 0 |
| MatchesPlayed | `int` | NOT NULL, ≥ 0 |
| Wins | `int` | NOT NULL, ≥ 0 |
| Draws | `int` | NOT NULL, ≥ 0 |
| Losses | `int` | NOT NULL, ≥ 0 |

**DB Constraints**:
- `UNIQUE(TournamentId, TeamId)`

**Lifecycle**: A `Standing` row is created (initialized to all zeros) when a team's `TournamentRegistration` becomes `Active`. It is updated — never deleted — when match results are recorded or corrected via full replay recalculation. If a team withdraws (`RegistrationStatus → Withdrawn`), its `Standing` row is deleted so it no longer appears in the standings table.

**Notes**: Position (rank) is computed at query time by `ORDER BY Points DESC`; not stored. Teams with equal `Points` share the same ordinal rank — no secondary tiebreaker (FR-031).

---

## Enums

```csharp
// Domain/Enums/UserRole.cs
enum UserRole { Player, Organizer, Admin }

// Domain/Enums/TournamentStatus.cs
enum TournamentStatus { Draft, Open, InProgress, Completed, Suspended }

// Domain/Enums/ScoringSystemType.cs
enum ScoringSystemType { Standard, WinnerTakesAll, Custom }

// Domain/Enums/InvitationStatus.cs
enum InvitationStatus { Pending, Accepted, Declined, Expired }

// Domain/Enums/RegistrationStatus.cs
enum RegistrationStatus { Active, Withdrawn }
```

---

## State Transition Diagrams

### Tournament Status

```
[Draft] ──Organizer advance──► [Open] ──Organizer advance──► [InProgress] ──Organizer advance──► [Completed]
   │                              │                               │
   │◄──Admin suspends (saves pre-suspension state)               │
   └──────────────────[Suspended]◄────────────────────────────────┘
            │
            └──Admin reinstates──► (restore PreSuspensionStatus)
```

- Forward transitions (Draft → Open → InProgress → Completed) are **unidirectional and forward-only** by the Organizer; states may be skipped.
- `Suspended` is entered from any non-`Completed` state by Admin action only; exit is also by Admin only.
- `PreSuspensionStatus` stores the state to restore on reinstatement.

### Invitation Status

```
[Pending] ──Player accepts──► [Accepted]
          ──Player declines──► [Declined]
          ──7 days elapsed (lazy)──► [Expired]
```

---

## Key Validation Rules Summary

| Rule | Enforcement Point |
|------|------------------|
| `Tournament.StartDate > UtcNow` at creation | Service layer |
| `Tournament.EstimatedEndDate > Tournament.StartDate` | Service layer |
| `Tournament.MaxTeams ≥ 2` | Service layer |
| `Tournament.MinMembersPerTeam ≥ 1` | Service layer |
| `ScoringSystem.WinPoints ≥ DrawPoints ≥ LossPoints ≥ 0` (Custom) | Service layer |
| Player cannot be Captain of > 1 team per game | Service layer |
| Player cannot be active member of > 1 team per game | Service layer |
| Tournament status transitions are forward-only (Organizer) | Service layer |
| `ScoringSystem` immutable once `InProgress` | Service layer |
| Registration only allowed when tournament is `Open` | Service layer |
| Team `VideogameId` must match Tournament `VideogameId` | Service layer |
| Duplicate match detection (ignoring home/away order) at same `PlayedAt` | Service layer |
| Organizer may only modify their own tournament | Service layer (ownership check) |
| Team name unique per `VideogameId` | DB constraint + service layer |
| `TournamentRegistration` unique per `(TournamentId, TeamId)` | DB constraint + service layer |

---

## EF Core Configuration Notes

- **Table-per-hierarchy (TPH)** for `User → Player / Organizer`: Use TPH with a discriminator column `UserType` (or split into separate tables using `Table-per-type (TPT)` if preferred for cleaner SQL queries — decide in implementation).
- **Owned entity**: Consider `ScoringSystem` as an owned entity on `Tournament` to simplify the 1:1 relationship, or keep it as a separate table with `UNIQUE(TournamentId)`.
- **Concurrency**: Use EF Core optimistic concurrency tokens on `Standing` rows to guard against race conditions during rapid result recording.
