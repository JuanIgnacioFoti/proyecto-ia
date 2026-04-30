# Data Model

**Feature**: Esports Tournament Platform  
**Date**: 2026-04-30  
**Database**: SQL Server with Entity Framework Core Code-First

## Entity Relationship Overview

```
User (abstract)
├── Organizer (owns) → Tournament (contains) → Match
├── Player (captains/belongs to) → Team (enrolls in) → Tournament
└── Player (receives) → TeamInvitation (from) → Team

Tournament → ScoringSystemType (enum, determines strategy)
Match → Team (home/away) → Player (members)
Standings (calculated view) ← Match (results)
```

## Core Entities

### User (Base Class - TPH Strategy)

**Purpose**: Base authentication entity for all platform users.

**Attributes**:
- `Id` (Guid, PK): Unique identifier
- `Email` (string, unique, indexed): Authentication identifier
- `PasswordHash` (string): BCrypt/PBKDF2 hashed password
- `Role` (UserRole enum): Organizer | Player
- `CreatedAt` (DateTime): Account creation timestamp
- `UpdatedAt` (DateTime, nullable): Last modification timestamp

**Validation Rules**:
- Email: unique, valid format, max 255 chars
- Password: 8+ chars, 1 uppercase, 1 lowercase, 1 number (validated before hashing)

**Relationships**:
- One-to-One with Organizer or Player (discriminator pattern)

**Indexes**:
- Unique index on Email
- Index on Role for filtered queries

---

### Organizer (Inherits User)

**Purpose**: Users who create and manage tournaments.

**Attributes**:
- `OrganizationName` (string, unique, indexed): Display name (3-60 chars)

**Validation Rules**:
- OrganizationName: unique, 3-60 chars, non-empty

**Relationships**:
- One-to-Many with Tournament (owns tournaments)

**Indexes**:
- Unique index on OrganizationName

---

### Player (Inherits User)

**Purpose**: Users who form teams and participate in tournaments.

**Attributes**:
- `Username` (string, unique, indexed): Display name
- `RealName` (string): Full legal name
- `MainGame` (string): Primary videogame

**Validation Rules**:
- Username: unique, non-empty, max 50 chars
- RealName: non-empty, max 100 chars
- MainGame: non-empty, max 50 chars

**Relationships**:
- One-to-Many with Team (as captain via CaptainId FK)
- Many-to-Many with Team (as member via TeamPlayer join table)
- One-to-Many with TeamInvitation (received invitations)

**Indexes**:
- Unique index on Username
- Index on MainGame for filtering

---

### Tournament

**Purpose**: Represents an esports competition event.

**Attributes**:
- `Id` (Guid, PK): Unique identifier
- `Name` (string): Tournament name (max 100 chars)
- `Game` (string): Videogame (max 50 chars, e.g., "League of Legends")
- `Description` (string, nullable): Tournament details (max 500 chars)
- `StartDate` (DateTime): Competition start (must be future at creation)
- `EndDate` (DateTime): Estimated completion (must be after StartDate)
- `MaxTeams` (int): Maximum participant teams (min 2)
- `MinPlayersPerTeam` (int): Minimum team size requirement (default 5)
- `ScoringSystemType` (ScoringSystemType enum): Standard | WTA | Custom
- `CustomWinPoints` (int, nullable): Points for win if Custom (default null)
- `CustomDrawPoints` (int, nullable): Points for draw if Custom (default null)
- `CustomLossPoints` (int, nullable): Points for loss if Custom (default null)
- `State` (TournamentState enum): Draft | Open | InProgress | Finished
- `OrganizerId` (Guid, FK): Owner organizer
- `CreatedAt` (DateTime): Creation timestamp
- `UpdatedAt` (DateTime, nullable): Last modification

**Validation Rules**:
- Name: non-empty, max 100 chars
- Game: non-empty, max 50 chars
- StartDate: must be > DateTime.UtcNow at creation
- EndDate: must be > StartDate
- MaxTeams: >= 2
- MinPlayersPerTeam: >= 1, default 5
- CustomPoints: required if ScoringSystemType == Custom
- State transitions: unidirectional (Draft → Open → InProgress → Finished)
- ScoringSystemType: immutable after State == InProgress

**Relationships**:
- Many-to-One with Organizer (owned by organizer)
- Many-to-Many with Team (via TournamentEnrollment join table)
- One-to-Many with Match (tournament matches)

**Indexes**:
- Index on OrganizerId (filter by organizer)
- Index on State (filter by state)
- Index on Game (filter by game)
- Composite index on (State, Game) for common query

**State Machine**:
```
Draft → Open → InProgress → Finished
  ↓      ↓          ↓
  └──────┴──────────┘  (can skip states but never go backward)
```

---

### Team

**Purpose**: Group of players competing together in a specific game.

**Attributes**:
- `Id` (Guid, PK): Unique identifier
- `Name` (string): Team name (max 60 chars)
- `Description` (string, nullable): Team details (max 300 chars)
- `Game` (string): Videogame (max 50 chars)
- `CaptainId` (Guid, FK): Team captain (must be a member)
- `CreatedAt` (DateTime): Team formation timestamp

**Validation Rules**:
- Name: unique within same Game, non-empty, max 60 chars
- Game: non-empty, max 50 chars
- Captain: must be a Player, one captain per team per game

**Relationships**:
- Many-to-One with Player (captain relationship via CaptainId)
- Many-to-Many with Player (members via TeamPlayer join table)
- Many-to-Many with Tournament (via TournamentEnrollment join table)
- One-to-Many with TeamInvitation (sent invitations)

**Indexes**:
- Unique composite index on (Name, Game)
- Index on CaptainId
- Index on Game

**Business Rules**:
- Captain must be a member of the team
- A player can captain only one team per game
- A player can belong to only one active team per game

---

### TeamPlayer (Join Table)

**Purpose**: Many-to-many relationship between Team and Player (team members).

**Attributes**:
- `TeamId` (Guid, FK): Team identifier
- `PlayerId` (Guid, FK): Player identifier
- `JoinedAt` (DateTime): Membership start timestamp

**Validation Rules**:
- Composite PK (TeamId, PlayerId) prevents duplicates
- Player can be in only one active team per game (enforced by business logic)

**Indexes**:
- Composite PK on (TeamId, PlayerId)
- Index on PlayerId for reverse lookup

---

### TeamInvitation

**Purpose**: Tracks invitations sent to players to join teams.

**Attributes**:
- `Id` (Guid, PK): Unique identifier
- `TeamId` (Guid, FK): Inviting team
- `InvitedPlayerId` (Guid, FK): Player being invited
- `Status` (InvitationStatus enum): Pending | Accepted | Rejected
- `CreatedAt` (DateTime): Invitation sent timestamp
- `RespondedAt` (DateTime, nullable): Acceptance/rejection timestamp

**Validation Rules**:
- Status: defaults to Pending
- Player cannot have multiple Pending invitations from same team
- Once Accepted/Rejected, immutable

**Relationships**:
- Many-to-One with Team (invitations from team)
- Many-to-One with Player (invitations to player)

**Indexes**:
- Index on TeamId
- Index on InvitedPlayerId
- Composite index on (InvitedPlayerId, Status) for fetching pending invitations
- Unique composite index on (TeamId, InvitedPlayerId, Status) when Status = Pending (prevents duplicate pending)

---

### TournamentEnrollment (Join Table)

**Purpose**: Many-to-many relationship between Tournament and Team.

**Attributes**:
- `TournamentId` (Guid, FK): Tournament identifier
- `TeamId` (Guid, FK): Enrolled team identifier
- `EnrolledAt` (DateTime): Enrollment timestamp

**Validation Rules**:
- Composite PK (TournamentId, TeamId) prevents duplicates
- Team game must match Tournament game
- Team must have >= Tournament.MinPlayersPerTeam
- Tournament must have < Tournament.MaxTeams enrolled
- Tournament.State must be Open

**Indexes**:
- Composite PK on (TournamentId, TeamId)
- Index on TeamId for reverse lookup

---

### Match

**Purpose**: Records a game between two teams in a tournament.

**Attributes**:
- `Id` (Guid, PK): Unique identifier
- `TournamentId` (Guid, FK): Tournament context
- `HomeTeamId` (Guid, FK): Home team
- `AwayTeamId` (Guid, FK): Away team
- `HomeScore` (int): Goals/points scored by home team
- `AwayScore` (int): Goals/points scored by away team
- `MatchDate` (DateTime): Match date/time
- `CreatedAt` (DateTime): Result registration timestamp

**Validation Rules**:
- HomeScore, AwayScore: >= 0
- HomeTeamId != AwayTeamId
- Both teams must be enrolled in tournament
- Unique constraint: (TournamentId, HomeTeamId, AwayTeamId, MatchDate) prevents duplicate matches
- Can only be registered by tournament organizer

**Relationships**:
- Many-to-One with Tournament
- Many-to-One with Team (home team)
- Many-to-One with Team (away team)

**Indexes**:
- Index on TournamentId
- Index on HomeTeamId
- Index on AwayTeamId
- Unique composite index on (TournamentId, HomeTeamId, AwayTeamId, MatchDate)

**Calculated Values** (not stored):
- Winner: determined by HomeScore vs AwayScore
- IsDraw: HomeScore == AwayScore

---

### Standings (Calculated View / Query Result)

**Purpose**: Tournament leaderboard calculated from match results.

**Attributes** (not persisted, calculated on demand):
- `TournamentId` (Guid): Tournament context
- `TeamId` (Guid): Team identifier
- `TeamName` (string): Team name
- `Position` (int): Rank (1-based, ordered by Points desc, then goal difference)
- `Points` (int): Total points calculated by scoring system
- `Played` (int): Total matches played (as home or away)
- `Won` (int): Matches won
- `Drawn` (int): Matches drawn
- `Lost` (int): Matches lost
- `GoalsFor` (int, optional): Total goals/points scored
- `GoalsAgainst` (int, optional): Total goals/points conceded
- `GoalDifference` (int, optional): GoalsFor - GoalsAgainst

**Calculation Logic**:
1. Query all matches for TournamentId
2. For each team in tournament enrollment:
   - Find all matches where TeamId = HomeTeamId OR TeamId = AwayTeamId
   - For each match, determine result (win/draw/loss) from scores
   - Apply tournament's IScoringSystem.CalculatePoints(result, side)
   - Aggregate: sum points, count won/drawn/lost, sum goals
3. Order by Points DESC, then GoalDifference DESC, then TeamName ASC

**Not an Entity**: This is a read-only query result, not stored in database. Recalculated on every standings request after match result changes.

---

## Enumerations

### UserRole
- `Organizer` = 0
- `Player` = 1

### TournamentState
- `Draft` = 0 (only visible to organizer)
- `Open` = 1 (teams can enroll)
- `InProgress` = 2 (enrollments closed, matches being played)
- `Finished` = 3 (tournament complete)

### ScoringSystemType
- `Standard` = 0 (3-1-0)
- `WTA` = 1 (3-0-0)
- `Custom` = 2 (organizer-defined)

### InvitationStatus
- `Pending` = 0
- `Accepted` = 1
- `Rejected` = 2

---

## EF Core Configuration Notes

### Table Per Hierarchy (TPH) for User Inheritance
```csharp
modelBuilder.Entity<User>()
    .HasDiscriminator<UserRole>("Role")
    .HasValue<Organizer>(UserRole.Organizer)
    .HasValue<Player>(UserRole.Player);
```

### Cascade Delete Behavior
- Tournament deleted → Matches deleted (cascade)
- Tournament deleted → Enrollments deleted (cascade)
- Team deleted → Enrollments deleted (cascade)
- Team deleted → Invitations deleted (cascade)
- Organizer deleted → Tournaments deleted (restrict - require manual cleanup)
- Player deleted → Team memberships deleted (restrict - require captain reassignment)

### Unique Constraints
- User.Email (unique)
- Organizer.OrganizationName (unique)
- Player.Username (unique)
- Team (Name, Game) composite unique
- Match (TournamentId, HomeTeamId, AwayTeamId, MatchDate) composite unique

### Value Objects (Consider for Future)
- Email (with validation)
- Password (with hashing)
- Score (with min/max validation)

Currently using primitive types for simplicity, can refactor to value objects if validation logic grows complex.

---

## Migration Strategy

1. **Initial Migration**: Create all tables, indexes, foreign keys
2. **Seed Data**: Optional seed for development (sample organizer, players, teams)
3. **Future Migrations**: Add fields, indexes, constraints as needed (track in git)

**Command**:
```bash
dotnet ef migrations add InitialCreate --project EsportsPlatform.Infrastructure
dotnet ef database update --project EsportsPlatform.API
```

---

## Summary

**Total Tables**: 9 core entities + 2 join tables = 11 tables
- User (with Organizer/Player TPH)
- Tournament
- Team
- Match
- TeamInvitation
- TeamPlayer (join)
- TournamentEnrollment (join)

**Total Enums**: 4
- UserRole, TournamentState, ScoringSystemType, InvitationStatus

**Calculated Views**: 1
- Standings (not persisted, query result)

**Key Relationships**:
- User → Organizer → Tournament (1:N)
- User → Player → Team (M:N via TeamPlayer)
- Player → Team (captain, 1:N)
- Team → Tournament (M:N via TournamentEnrollment)
- Tournament → Match (1:N)
- Team → TeamInvitation (1:N)
- Player → TeamInvitation (1:N)

**All entities support spec requirements (FR-001 through FR-043). Ready for Phase 1 contract definitions.**
