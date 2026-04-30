# Feature Specification: Esports Tournament Platform

**Feature Branch**: `001-esports-tournament-platform`  
**Created**: 2026-04-30  
**Status**: Draft  
**Input**: User description: "Plataforma de Campeonatos de Esports - Sistema completo de gestión de torneos de videojuegos"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Tournament Organizer Registration and Tournament Creation (Priority: P1)

An entertainment company representative wants to organize esports tournaments on the platform. They need to register as an organizer, create tournaments with specific rules, and manage the tournament lifecycle.

**Why this priority**: Core value proposition - without organizers creating tournaments, there is no platform functionality.

**Independent Test**: Can be fully tested by creating an organizer account, logging in, creating a tournament in draft state, and verifying it's visible only to the organizer. Delivers immediate value: organizers can start planning their events.

**Acceptance Scenarios**:

1. **Given** I am a new organizer, **When** I register with organization name "ESL Gaming" (unique, 3-60 chars), email "admin@eslgaming.com" (valid, unique), and password "SecurePass1" (8+ chars, 1 uppercase, 1 lowercase, 1 number), **Then** my organizer account is created successfully
2. **Given** I am a registered organizer, **When** I attempt to register again with the same email or organization name, **Then** I receive a validation error indicating the duplicate field
3. **Given** I am logged in as an organizer, **When** I create a tournament with name "League of Legends Championship", game "League of Legends", description, future start date, later end date, max 16 teams, min 5 players per team, and standard scoring system, **Then** the tournament is created in "draft" state and visible only to me
4. **Given** I am an organizer with a tournament in draft state, **When** I modify tournament details (name, description, dates, max teams, min players, scoring system), **Then** my changes are saved successfully
5. **Given** I am an organizer, **When** I transition my tournament from draft → open, **Then** teams can now see and enroll in the tournament
6. **Given** I am an organizer, **When** I attempt to create a tournament with a start date in the past or an end date before the start date, **Then** I receive a validation error

---

### User Story 2 - Player Registration and Team Formation (Priority: P1)

Players want to register on the platform, create teams for their favorite games, invite teammates, and manage team composition.

**Why this priority**: Without players and teams, there are no participants for tournaments. This is essential for platform adoption.

**Independent Test**: Can be fully tested by creating a player account, forming a team, inviting another player, and verifying team composition. Delivers standalone value: players can organize their squads.

**Acceptance Scenarios**:

1. **Given** I am a new player, **When** I register with username "ProGamer123" (unique), real name "John Doe", email "john@example.com" (unique, valid), password "MyPass123", and main game "Valorant", **Then** my player account is created successfully
2. **Given** I am a registered player, **When** I create a team with name "Team Phoenix" (unique within Valorant), description, and game "Valorant", **Then** I become the team captain automatically
3. **Given** I am a team captain, **When** I invite a registered player by email or username to my team, **Then** they receive an invitation
4. **Given** I am a player with a pending team invitation, **When** I accept the invitation, **Then** I join the team (if I don't already belong to another active team for that game)
5. **Given** I am a player with a pending team invitation, **When** I reject the invitation, **Then** I remain without a team
6. **Given** I am a team captain, **When** I remove a player from my team, **Then** they are no longer part of the team
7. **Given** I am a player, **When** I attempt to join a second active team for the same game, **Then** I receive an error indicating I can only belong to one team per game
8. **Given** I am a player, **When** I attempt to create a second team as captain for the same game, **Then** I receive an error indicating I can only captain one team per game

---

### User Story 3 - Team Tournament Enrollment (Priority: P1)

Team captains want to enroll their teams in open tournaments matching their game, ensuring they meet eligibility requirements.

**Why this priority**: This connects players/teams with tournaments, completing the core platform loop.

**Independent Test**: Can be fully tested by creating a team with sufficient players, finding an open tournament for the same game, enrolling, and verifying the enrollment appears in the tournament participant list.

**Acceptance Scenarios**:

1. **Given** I am a captain of a team with 5+ players, **When** I enroll my team in an open tournament for the same game with available slots, **Then** my team is successfully enrolled
2. **Given** I am a captain of a team with less than the minimum required players, **When** I attempt to enroll in a tournament, **Then** I receive an error indicating insufficient team members
3. **Given** I am a captain, **When** I attempt to enroll my team in a tournament that has reached maximum capacity, **Then** I receive an error indicating the tournament is full
4. **Given** I am a captain, **When** I attempt to enroll my already-enrolled team in the same tournament again, **Then** I receive an error indicating duplicate enrollment
5. **Given** I am a captain of a Valorant team, **When** I attempt to enroll in a League of Legends tournament, **Then** I receive an error indicating game mismatch

---

### User Story 4 - Match Results Registration and Standings Calculation (Priority: P2)

Organizers need to register match results as tournaments progress, with the system automatically calculating and updating standings based on the configured scoring system.

**Why this priority**: Enables tournament execution and real-time competition tracking, which is core to the esports experience.

**Independent Test**: Can be fully tested by starting a tournament, registering results between two enrolled teams, and verifying standings update automatically with correct points according to the scoring system.

**Acceptance Scenarios**:

1. **Given** I am an organizer with a tournament in "in progress" state, **When** I register a match result with home team, away team, score (e.g., 3-1), and date/time, **Then** the result is saved and standings are automatically recalculated
2. **Given** standings are being calculated with standard scoring (3-1-0), **When** Team A wins 2-1 against Team B, **Then** Team A gains 3 points and Team B gains 0 points
3. **Given** standings are being calculated with standard scoring (3-1-0), **When** Team A draws 1-1 against Team B, **Then** both teams gain 1 point
4. **Given** standings are being calculated with WTA scoring (3-0-0), **When** Team A wins against Team B, **Then** Team A gains 3 points and Team B gains 0 points
5. **Given** standings are being calculated with WTA scoring (3-0-0), **When** Team A draws with Team B, **Then** both teams gain 0 points
6. **Given** I am an organizer, **When** I attempt to register a match between the same two teams at the same date/time, **Then** I receive an error indicating duplicate match
7. **Given** I am an organizer, **When** I attempt to register a match result for a tournament I don't own, **Then** I receive a 403 Forbidden error

---

### User Story 5 - Viewing Tournament Standings (Priority: P2)

Any authenticated player wants to view real-time standings for ongoing or completed tournaments to track competition progress.

**Why this priority**: Provides transparency and engagement for participants and spectators.

**Independent Test**: Can be fully tested by logging in as any player, navigating to a tournament in progress or completed, and viewing the standings table with rankings, points, wins, draws, losses.

**Acceptance Scenarios**:

1. **Given** I am an authenticated player, **When** I view the standings of an ongoing tournament, **Then** I see: position, team name, points, matches played, wins, draws, losses
2. **Given** I am an authenticated player, **When** I view the standings of a completed tournament, **Then** I see the final rankings with complete statistics
3. **Given** I am not authenticated, **When** I attempt to view tournament standings, **Then** I receive a 401 Unauthorized error

---

### User Story 6 - Tournament State Transitions (Priority: P2)

Organizers need to control tournament lifecycle by transitioning between states (draft → open → in progress → finished) with proper validation and restrictions.

**Why this priority**: Ensures tournament integrity and prevents invalid operations at wrong stages.

**Independent Test**: Can be fully tested by creating a tournament and performing valid state transitions, verifying that enrollments are blocked when "in progress", and that backward transitions are rejected.

**Acceptance Scenarios**:

1. **Given** I have a tournament in "draft" state, **When** I transition it to "open", **Then** teams can now enroll
2. **Given** I have a tournament in "open" state with enrolled teams, **When** I transition it to "in progress", **Then** no more enrollments are accepted
3. **Given** I have a tournament in "in progress" state, **When** I transition it to "finished", **Then** the tournament is marked as completed
4. **Given** I have a tournament in "open" state, **When** I attempt to transition backward to "draft", **Then** I receive an error indicating backward transitions are not allowed
5. **Given** I have a tournament in "draft" state, **When** I skip directly to "in progress", **Then** the transition succeeds (state skipping is allowed)
6. **Given** I have a tournament in "in progress" or "finished" state, **When** I attempt to modify the scoring system, **Then** I receive an error indicating the scoring system is immutable after starting

---

### User Story 7 - Account Modification (Priority: P3)

Users (organizers and players) want to update their account information to keep their profiles current.

**Why this priority**: Nice-to-have for user experience but not critical for core platform functionality.

**Independent Test**: Can be fully tested by logging in as an organizer or player, updating account fields, and verifying changes persist.

**Acceptance Scenarios**:

1. **Given** I am a logged-in organizer, **When** I update my organization name and password, **Then** my changes are saved successfully
2. **Given** I am a logged-in player, **When** I update my real name, email, and password, **Then** my changes are saved successfully
3. **Given** I am updating my account, **When** I attempt to use an email or username already taken by another user, **Then** I receive a validation error

---

### Edge Cases

- What happens when an organizer tries to delete a tournament with enrolled teams?
- How does the system handle concurrent match result registrations for the same teams?
- What happens when a player is removed from a team after the team is enrolled in a tournament?
- How does the system handle custom scoring values that are negative or zero?
- What happens when a tournament with no matches transitions to "finished"?
- How does the system handle timezone differences for match dates/times?
- What happens if an organizer account is deleted while owning active tournaments?

## Requirements *(mandatory)*

### Functional Requirements

#### Authentication & Authorization

- **FR-001**: System MUST implement user authentication (mechanism to be determined by team)
- **FR-002**: System MUST return 401 Unauthorized for unauthenticated requests to protected endpoints
- **FR-003**: System MUST return 403 Forbidden for authenticated requests without required role
- **FR-004**: System MUST support two user roles: Organizer and Player

#### Organizer Management

- **FR-005**: System MUST allow organizer registration with: organization name (unique, 3-60 chars), email (unique, valid format), password (8+ chars, 1 uppercase, 1 lowercase, 1 number)
- **FR-006**: System MUST allow organizers to modify their organization name and password
- **FR-007**: System MUST prevent duplicate organization names across all organizers
- **FR-008**: System MUST prevent duplicate emails across all users (organizers and players)

#### Tournament Management

- **FR-009**: System MUST allow organizers to create tournaments with: name, game, description, start date (future), end date (after start date), max teams (min 2), min players per team (default 5), scoring system (standard/WTA/custom), state
- **FR-010**: System MUST validate that tournament start date is in the future at creation time
- **FR-011**: System MUST validate that tournament end date is after start date
- **FR-012**: System MUST allow organizers to modify tournament details only for their own tournaments
- **FR-013**: System MUST enforce tournament state transitions: draft → open → in progress → finished (unidirectional, can skip states, cannot go backward)
- **FR-014**: System MUST make draft tournaments visible only to their creator
- **FR-015**: System MUST make open/in progress/finished tournaments visible to all authenticated users
- **FR-016**: System MUST prevent modifications to scoring system after tournament starts (in progress or finished)
- **FR-017**: System MUST prevent team enrollments when tournament is "in progress" or "finished"

#### Match Results & Scoring

- **FR-018**: System MUST allow organizers to register match results with: home team, away team, score (goals/points each), date/time
- **FR-019**: System MUST prevent duplicate matches (same two teams at same date/time)
- **FR-020**: System MUST automatically calculate and update standings when match results are registered
- **FR-021**: System MUST support Standard scoring: 3 points win, 1 point draw, 0 points loss
- **FR-022**: System MUST support WTA (Winner Takes All) scoring: 3 points win, 0 points draw/loss
- **FR-023**: System MUST support Custom scoring: organizer-defined values for win/draw/loss
- **FR-024**: System MUST allow adding new scoring systems without modifying existing code (Strategy Pattern)
- **FR-025**: System MUST ensure organizers can only register results for their own tournaments

#### Player Management

- **FR-026**: System MUST allow player registration with: username (unique), real name, email (unique, valid), password (8+ chars, 1 uppercase, 1 lowercase, 1 number), main game
- **FR-027**: System MUST allow players to modify their real name, email, and password
- **FR-028**: System MUST prevent duplicate usernames across all players

#### Team Management

- **FR-029**: System MUST allow players to create teams, becoming captain automatically
- **FR-030**: System MUST require teams to have: name (unique within game), description, game
- **FR-031**: System MUST enforce that a player can captain only one team per game
- **FR-032**: System MUST allow captains to invite players to their team by email or username
- **FR-033**: System MUST allow invited players to accept or reject team invitations
- **FR-034**: System MUST allow captains to remove players from their team
- **FR-035**: System MUST enforce that a player can belong to only one active team per game

#### Tournament Enrollment

- **FR-036**: System MUST allow team captains to enroll their team in open tournaments of the same game
- **FR-037**: System MUST validate enrollment requirements: tournament not full, team has minimum players, team not already enrolled, game matches
- **FR-038**: System MUST prevent enrollment when tournament has reached max teams capacity
- **FR-039**: System MUST prevent enrollment when team has fewer than tournament's minimum required players
- **FR-040**: System MUST prevent duplicate enrollments of the same team in the same tournament

#### Standings & Reporting

- **FR-041**: System MUST allow authenticated users to view standings for any tournament in progress or finished
- **FR-042**: System MUST display standings with: position, team name, points, matches played, wins, draws, losses
- **FR-043**: System MUST order standings by points (descending), then by goal difference (if applicable)

### Key Entities

- **User**: Represents both organizers and players, with role differentiation. Attributes: email, password (hashed), role (organizer/player)
- **Organizer**: Extends User. Attributes: organization name. Relationships: owns multiple tournaments
- **Player**: Extends User. Attributes: username, real name, main game. Relationships: captains teams, belongs to teams, receives invitations
- **Team**: Represents a group of players for a specific game. Attributes: name, description, game, captain (player). Relationships: has multiple players, enrolls in tournaments
- **TeamInvitation**: Represents invitation sent to a player. Attributes: team, invited player, status (pending/accepted/rejected)
- **Tournament**: Represents an esports competition. Attributes: name, game, description, start date, end date, max teams, min players per team, scoring system, state (draft/open/in progress/finished), organizer. Relationships: has enrolled teams, has matches
- **Match**: Represents a game between two teams. Attributes: tournament, home team, away team, home score, away score, date/time
- **Standings**: Calculated from match results. Attributes: tournament, team, position, points, played, wins, draws, losses
- **ScoringSystem**: Interface/Strategy for calculating points. Implementations: StandardScoring, WTAScoring, CustomScoring

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Organizers can create and publish a tournament in under 5 minutes
- **SC-002**: Players can register, create a team, and enroll in an open tournament in under 10 minutes
- **SC-003**: Match results are reflected in standings within 2 seconds of registration
- **SC-004**: System handles 100 concurrent organizers creating tournaments without performance degradation
- **SC-005**: System handles 1,000 concurrent players viewing standings without performance degradation
- **SC-006**: Tournament state transitions are validated with 100% accuracy (no invalid transitions allowed)
- **SC-007**: Scoring system calculations are accurate for 100% of match results (verified through unit tests)
- **SC-008**: Enrollment validation prevents 100% of invalid enrollments (wrong game, insufficient players, duplicate, tournament full)
- **SC-009**: Authentication and authorization correctly block 100% of unauthorized access attempts (401/403 responses)
- **SC-0 10**: New scoring systems can be added without modifying any existing code (verified through code review and extensibility tests)

## Assumptions

- Users have stable internet connectivity and modern web browsers (Chrome, Firefox, Edge, Safari - latest versions)
- Tournament organizers are responsible for scheduling matches externally; the platform only records results
- Real-time notifications for team invitations are out of scope for v1 (invitations visible on next login)
- Mobile-responsive design is required but native mobile apps are out of scope
- Single language support (Spanish for error messages, English for code and API)
- Timezone handling: all dates/times stored in UTC, displayed in user's local timezone
- Payment processing for tournament entry fees is out of scope for v1
- Live streaming integration is out of scope for v1
- Bracket/fixture generation is out of scope for v1 (organizers manually register match results)
- File uploads (team logos, tournament banners) are out of scope for v1
- Social features (chat, comments, follows) are out of scope for v1
- Admin panel for platform administrators is out of scope for v1
- System scales to support up to 1,000 concurrent users (sufficient for MVP)
- SQL Server is used as the database with Entity Framework Core Code-First approach
- .NET 10 and Angular 21 are mandated by academic requirements
- GitHub is used for version control and GitHub Issues for feature tracking
