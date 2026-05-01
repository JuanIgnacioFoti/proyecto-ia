# Feature Specification: Esports Tournament Management Platform

**Feature Branch**: `001-esports-tournament-platform`  
**Created**: 2026-04-20  
**Status**: Draft  
**Input**: User description: "Plataforma web para organizar y gestionar campeonatos de videojuegos (esports)"

---

## Clarifications

### Session 2026-04-20

- Q: When two teams have equal total points in the standings, what tiebreaker rule determines their relative ranking? → A: No tiebreaker — tied teams share the same position (e.g., both ranked 3rd).
- Q: How long should a JWT session token remain valid before expiring? → A: 24 hours.
- Q: How should real-time standings updates be delivered to the client? → A: SignalR (WebSocket hub in ASP.NET Core, Angular client listens for push events).
- Q: How is the Videogame catalog managed — who can add new game titles to the platform? → A: Seeded at startup with a fixed list; no runtime additions.
- Q: When an Admin suspends an Organizer account, what happens to that Organizer's active tournaments? → A: Tournaments enter a "Suspended" state; no changes, registrations, or results can be recorded until resolved by an Admin.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registration and Login (Priority: P1)

The platform has two distinct self-registration flows: one for **Players** and one for **Organizers**. A Player registers with a unique username, their real name, a unique email, a password, and their main videogame. An Organizer registers with a unique organization name, a valid email, and a password that meets complexity requirements. After registration, either type of user logs in with their credentials and receives a session token that grants access to protected features. Authenticated users can also update certain account fields.

**Why this priority**: Authentication is the foundation of the entire platform. Without it, no other feature — team management, tournament creation, or result tracking — can function in a role-controlled way. Every other story depends on knowing who the user is and what they are allowed to do.

**Independent Test**: Can be fully tested by registering one Player account and one Organizer account (verifying field validation on each), logging in with each, accessing a protected endpoint as each role, verifying that a request without credentials returns HTTP 401 and a request with the wrong role returns HTTP 403, and verifying that a Player can update their real name, email, and password.

**Acceptance Scenarios**:

1. **Given** a visitor with no account, **When** they submit valid Player registration data (unique username, real name, unique valid email, password, and main videogame), **Then** a Player account is created and they can immediately log in.
2. **Given** a visitor attempting to register as a Player with a username or email already in use, **When** they submit the form, **Then** the system rejects the request with a validation error identifying the duplicate field.
3. **Given** a visitor with no account, **When** they submit valid Organizer registration data (unique organization name between 3 and 60 characters, unique valid email, and a password of at least 8 characters containing at least one uppercase letter, one lowercase letter, and one digit), **Then** an Organizer account is created and they can immediately log in.
4. **Given** a visitor attempting to register as an Organizer with an organization name already in use, **When** they submit the form, **Then** the system rejects the request with a validation error identifying the duplicate name.
5. **Given** a visitor attempting to register with an email already in use, **When** they submit the form, **Then** the system rejects the request with a validation error identifying the duplicate email.
6. **Given** a visitor attempting to register as an Organizer with a password shorter than 8 characters or missing a required character type, **When** they submit the form, **Then** the system rejects the request with a validation error describing the violated rule.
7. **Given** an authenticated Player, **When** they update their real name, email, or password with valid values, **Then** the changes are persisted and take effect on the next login if the email was changed.
8. **Given** a registered user, **When** they submit correct credentials, **Then** they receive a valid session token that allows access to protected resources.
9. **Given** a valid session token, **When** the user accesses a resource requiring a role they do not hold, **Then** the system returns HTTP 403.
10. **Given** no session token, **When** the user attempts to access any protected resource, **Then** the system returns HTTP 401.
11. **Given** a registered user, **When** they submit incorrect credentials, **Then** login is refused and no token is issued.
12. **Given** an expired or tampered token, **When** it is used for a request, **Then** the system returns HTTP 401.

---

### User Story 2 - Tournament Creation and Management (Priority: P2)

An Organizer creates a new league tournament for a specific videogame, providing a name, description, future start date, estimated end date, team capacity, minimum team size, and a scoring system. The tournament begins in **Draft** status and is visible only to its Organizer. The Organizer manually advances it through its lifecycle — Draft → Open → In Progress → Completed — at their own pace. State transitions are always forward; states can be skipped but never reversed.

**Why this priority**: Tournaments are the platform's core offering. Without the ability to create and manage them, no team can register and no results can be tracked. This story delivers a self-contained MVP for the Organizer persona.

**Independent Test**: Can be tested by logging in as an Organizer, creating a tournament (verifying it appears in Draft only for that Organizer and not for other users), advancing it to Open, verifying public visibility, and verifying another Organizer or Player cannot modify it (HTTP 403).

**Acceptance Scenarios**:

1. **Given** an authenticated Organizer, **When** they create a tournament providing name, videogame, description, a future start date, an estimated end date after the start date, a maximum team count of at least 2, a minimum members per team (defaulting to 5 if not specified), and a scoring system, **Then** the tournament is created with status "Draft" and is only visible to that Organizer.
2. **Given** a tournament in "Draft" or "Open" status, **When** the Organizer updates editable fields (name, description, dates, team limits, minimum members per team), **Then** the changes are persisted.
3. **Given** a tournament in any status before "Completed", **When** the Organizer advances its status, **Then** the status moves to the next or any later state in the sequence (Draft → Open → In Progress → Completed); the status can never revert to a previous state.
4. **Given** a tournament in "Draft" status, **When** any user other than the owning Organizer attempts to view it, **Then** it does not appear in tournament listings or detail views.
5. **Given** a tournament advanced to "Open" status, **When** any visitor views the tournament list, **Then** the tournament appears publicly.
6. **Given** a tournament in "In Progress" or later status, **When** the Organizer attempts to change the scoring system, **Then** the system rejects the change.
7. **Given** an authenticated Player or a different Organizer, **When** they attempt to modify another Organizer's tournament, **Then** the system returns HTTP 403.
8. **Given** an unauthenticated visitor, **When** they attempt to create a tournament, **Then** the system returns HTTP 401.

---

### User Story 3 - Team Formation and Player Management (Priority: P3)

A Player creates a team for a specific videogame (the team's main videogame must match the Player's main videogame), providing a unique name within that videogame and a description. The Player automatically becomes the team's Captain. A Player can only be Captain of one team per videogame. The Captain can invite other Players by email or username, and can remove members. Invited Players accept or decline. A Player can only be an active member of one team per videogame.

**Why this priority**: Teams are the competitive unit in the platform. Before any team can enter a tournament, it must exist. This story enables the team-building workflow that feeds into tournament registration.

**Independent Test**: Can be tested by creating two Player accounts for the same videogame, having one create a team (becoming Captain), inviting the other by username, verifying the roster, and then verifying that a second team for the same videogame cannot include the already-membered Player.

**Acceptance Scenarios**:

1. **Given** an authenticated Player, **When** they create a team providing a name unique within that videogame, a description, and the main videogame, **Then** the team is created with that Player as Captain.
2. **Given** a Player who is already a Captain of a team for a given videogame, **When** they attempt to create a second team for the same videogame, **Then** the system rejects the attempt.
3. **Given** a Team Captain, **When** they invite a Player by email or username who is not already an active member of a team for that videogame, **Then** the invitation is sent and the Player can accept or decline.
4. **Given** a Player who has accepted a team invitation, **When** another team for the same videogame tries to include them, **Then** the system rejects the attempt.
5. **Given** a Team Captain, **When** they remove a Player from the team, **Then** the Player is no longer part of the team and is free to join another.
6. **Given** a Team Captain, **When** they transfer captaincy to another team member, **Then** that member becomes the new Captain.
7. **Given** an unauthenticated visitor, **When** they attempt to create or manage a team, **Then** the system returns HTTP 401.

---

### User Story 4 - Tournament Registration (Priority: P4)

A Team Captain registers their team in a tournament for the same videogame their team is associated with. The system validates eligibility (correct game, tournament open, team not already registered, slot available). The Captain can withdraw the registration while the tournament is still open.

**Why this priority**: Registration is the link between teams and tournaments. Without it, the competitive structure cannot be formed, brackets cannot be generated, and match scheduling cannot begin.

**Independent Test**: Can be tested by creating a team for game X, creating a tournament for game X (in "Open" status), having the Captain register the team, and verifying the team appears in the tournament's participant list. Attempting to register the same team again or a team for a different game must fail.

**Acceptance Scenarios**:

1. **Given** a Team Captain and an open tournament for the same videogame as their team, where the team meets all eligibility criteria, **When** the Captain submits a registration, **Then** the team is added to the tournament's participant list.
2. **Given** a Team Captain attempting to register in a tournament for a different videogame, **When** the request is submitted, **Then** the system rejects it with a validation error.
3. **Given** a team already registered in a tournament, **When** the Captain attempts to register again in the same tournament, **Then** the system rejects the duplicate registration.
4. **Given** a tournament that has reached its maximum team count, **When** a new registration is attempted, **Then** the system rejects it.
5. **Given** a team that does not meet the tournament's minimum member count, **When** the Captain attempts to register, **Then** the system rejects it with a validation error stating the minimum members required.
6. **Given** a Team Captain who has registered their team, **When** the tournament is still open and they withdraw, **Then** the team is removed and the slot becomes available.
7. **Given** a tournament with status "In Progress" or "Completed", **When** any registration or withdrawal is attempted, **Then** the system rejects it.

---

### User Story 5 - Results and Standings Viewing (Priority: P5)

Any visitor can browse the tournament list and see match results. Authenticated users additionally have access to the full standings table for any tournament that is in progress or completed. The standings table shows, for each team: their position, team name, total points, matches played, matches won, matches drawn, and matches lost.

**Why this priority**: Spectator access drives platform engagement and reach. It is independently deliverable as a read-only view layer once tournaments and matches exist.

**Independent Test**: Can be tested by opening the tournament list as an anonymous user (tournaments visible), then opening a tournament's match history (visible), then attempting to view the standings table without authentication (should be rejected or hidden), and finally viewing the standings as a logged-in Player (fully visible with all columns).

**Acceptance Scenarios**:

1. **Given** any visitor, **When** they navigate to the tournament list, **Then** they see all Open, In Progress, and Completed tournaments with their current status.
2. **Given** any visitor, **When** they open a tournament page, **Then** they see the list of registered teams and the match history with home team, away team, score, and date/time.
3. **Given** an authenticated user, **When** they view a tournament in "In Progress" or "Completed" status, **Then** they see the standings table with, for each team: position, team name, total points, matches played, matches won, matches drawn, and matches lost.
4. **Given** a match result that has just been recorded by the Organizer, **When** an authenticated user views the tournament page, **Then** the updated standings and match result appear within 5 seconds without a full page reload.
5. **Given** a completed tournament, **When** an authenticated Player views it, **Then** the final standings and all match results are permanently accessible.

---

### User Story 6 - Match Result Recording (Priority: P6)

During a league tournament in progress, the Organizer records the outcome of each match by specifying the home team, away team, the score (goals or points per team), and the date and time the match took place. The system automatically calculates and updates the standings table using the tournament's configured scoring system. The Organizer can correct a previously recorded result as long as the tournament is not yet completed.

**Why this priority**: Result recording is what drives the league standings. Without it, the competitive progression cannot be tracked or concluded.

**Independent Test**: Can be tested by creating a tournament with at least two registered teams, starting it, recording two matches as the Organizer, and verifying that the standings table reflects the correct accumulated points per the configured scoring system. Attempting to record results as a Player must return HTTP 403.

**Acceptance Scenarios**:

1. **Given** a tournament "In Progress" and an authenticated Organizer who owns it, **When** they record a match providing home team, away team, score (goals/points per team), and date/time, **Then** the result is persisted and the standings table is automatically recalculated using the tournament's scoring system.
2. **Given** a previously recorded match result, **When** the Organizer corrects the score (before the tournament is completed), **Then** the correction is persisted and the standings table is fully recalculated to reflect the change.
3. **Given** the same two teams already have a match recorded with the same date and time, **When** the Organizer attempts to record another match with those two teams at that same date/time, **Then** the system rejects the request with a validation error.
4. **Given** an authenticated Player or an Organizer who does not own the tournament, **When** they attempt to record or modify a result, **Then** the system returns HTTP 403.
5. **Given** a tournament with status "Completed", **When** any user attempts to record a new match result, **Then** the system rejects the request.

---

### Edge Cases

- What happens when a Team Captain leaves their own team? → Captaincy must be transferred to another member before leaving, or the team is disbanded if no other members exist.
- What happens when the Organizer tries to advance a tournament that has no registered teams to "In Progress"? → The system allows the transition; ensuring adequate participation is the Organizer's responsibility.
- What happens if a team disbands after registering for a tournament that has not yet started? → The registration is automatically withdrawn and the slot becomes available.
- What happens if a team had enough members at registration time but loses members before the tournament starts? → The team remains registered; minimum member count is validated only at the moment of registration.
- What happens if a Custom scoring system is configured with values that make standings ambiguous (e.g., win < draw)? → The system must validate that win ≥ draw ≥ loss and all values are non-negative integers at creation time.
- What happens when the Organizer tries to record a match between the same two teams at the same date and time as an existing result? → The system rejects the request with a validation error; the existing result is unaffected.
- What happens when the Organizer records a match score where both teams have the same number of goals/points? → The result is a draw; the scoring system distributes draw points to both teams accordingly.
- What happens when an Organizer account is deleted or suspended? → When suspended by an Admin, all of that Organizer's tournaments that are not yet Completed automatically enter a **Suspended** state: no changes, registrations, or results can be recorded. When the Admin reinstates the Organizer, affected tournaments return to their pre-suspension state. Account deletion is out of scope; only suspension and reinstatement are supported.
- What happens if a player tries to register for a tournament directly without a team? → The system rejects the attempt; only Team Captains may register teams.
- What happens when an Organizer tries to change their organization name to one already in use? → The system rejects the change with a validation error identifying the duplicate name.
- What happens when the Organizer provides a start date in the past when creating a tournament? → The system rejects the creation with a validation error.

---

## Requirements *(mandatory)*

### Functional Requirements

#### Authentication & Authorization

- **FR-001**: The system MUST allow visitors to self-register as a **Player** by providing: unique username, real name, unique email address (valid format), password, and main videogame.
- **FR-002**: The system MUST authenticate users via a JWT token-based mechanism; the token must be included in subsequent requests to access protected resources. JWT tokens MUST expire after **24 hours** from the time of issuance; an expired token MUST be rejected with HTTP 401.
- **FR-003**: The system MUST enforce role-based access control; each endpoint that requires authorization must validate the caller's role before processing the request.
- **FR-004**: Any request to a protected resource without a valid authentication token MUST be rejected with HTTP 401.
- **FR-005**: Any request to a resource for which the authenticated user lacks the required role MUST be rejected with HTTP 403.
- **FR-006**: The system MUST support four distinct roles: **Player**, **Team Captain**, **Organizer**, and **Admin**. A Player who creates a team automatically assumes the Team Captain role for that team.

#### User & Role Management

- **FR-007**: Users self-register into one of two roles: **Player** (via the player registration flow, FR-001) or **Organizer** (via the organizer registration flow, FR-036). The **Team Captain** role is automatically assumed by a Player who creates a team and is not a separate registration. The **Admin** role is reserved and cannot be obtained through self-registration.
- **FR-008**: An **Admin** MUST be able to list all users, change their roles (except the Admin role of other Admins), and suspend or reinstate Organizer accounts. Suspending an Organizer MUST automatically move all of that Organizer's tournaments that are not yet Completed into a **Suspended** state. Reinstating the Organizer returns those tournaments to their pre-suspension state.
- **FR-009**: A **Player** MUST be able to view their own profile.
- **FR-038**: A **Player** MUST be able to modify their own account, limited to: real name, email address (subject to the same uniqueness and format rules as FR-001), and password. Username and main videogame cannot be changed after registration.

#### Organizer Registration & Account Management

- **FR-036**: The system MUST allow visitors to self-register as an **Organizer** by providing:
  - **Organization name**: unique across all Organizer accounts, minimum 3 characters, maximum 60 characters.
  - **Email address**: unique across all accounts, must be a valid email format.
  - **Password**: minimum 8 characters, must contain at least one uppercase letter, at least one lowercase letter, and at least one digit.
- **FR-037**: An **Organizer** MUST be able to modify their own account, limited to: organization name (subject to the same uniqueness and length rules as FR-036) and password (subject to the same complexity rules as FR-036). The email address cannot be changed after registration.

#### Tournament Management

- **FR-010**: An **Organizer** MUST be able to create a league (round-robin) tournament specifying: name, videogame, description, start date (must be strictly in the future at the moment of creation), estimated end date (must be after the start date), maximum number of teams (minimum 2), minimum number of members per team (minimum 1, default 5), and a scoring system.
- **FR-011**: An **Organizer** MUST only be able to create, edit, and delete tournaments they own.
- **FR-012**: A tournament has five lifecycle states: **Draft**, **Open**, **In Progress**, **Completed**, and **Suspended**. The following rules apply:
  - **Draft**: The tournament is only visible to its owning Organizer. Team registration is not permitted.
  - **Open**: The tournament is publicly visible. Teams may register.
  - **In Progress**: No new team registrations are accepted. Match results can be recorded.
  - **Completed**: The tournament is closed. No further result changes are permitted.
  - **Suspended**: Entered automatically when the owning Organizer's account is suspended by an Admin. No registrations, status advances, or result changes are permitted. The tournament returns to its pre-suspension state when the Organizer is reinstated. A tournament that was publicly visible (Open, In Progress, or Completed) before suspension MUST remain visible in public listings with a **"Suspended" badge** clearly indicating its status; it MUST NOT disappear from listings.
  - Normal state transitions (Draft → Open → In Progress → Completed) are **unidirectional and forward-only**: the Organizer may advance the status to the next state or skip to any later state, but can never revert to a previous state. The Suspended state is set and cleared exclusively by Admin action and does not participate in the forward-only sequence.
- **FR-013**: *(Reserved — removed: bracket generation does not apply to the league format.)*
- **FR-014**: The system MUST prevent registration of new teams once a tournament's status is "In Progress" or later.
- **FR-015**: Unauthenticated visitors MUST be able to view tournament listings (Open, In Progress, Completed only) and tournament details including the registered team list and match history (home team, away team, score, date/time). Tournaments in "Draft" status MUST NOT appear to any user other than the owning Organizer.
- **FR-039**: Any **authenticated** user MUST be able to view the standings table for any tournament in "In Progress" or "Completed" status. The standings table MUST display, for each team: position (ranked by points, descending), team name, total points, matches played, matches won, matches drawn, and matches lost.

#### Team Management

- **FR-016**: A **Player** MUST be able to create a team by providing: a name (unique within the selected videogame), a description, and the main videogame. The creating Player automatically becomes the team's **Team Captain**. A Player MUST NOT be the Captain of more than one team per videogame.
- **FR-017**: A **Team Captain** MUST be able to invite registered Players to join their team by specifying the target Player's username or email address.
- **FR-018**: A **Player** MUST be able to accept or decline a team invitation.
- **FR-019**: A **Player** MUST NOT be a member of more than one team for the same videogame simultaneously.
- **FR-020**: A **Team Captain** MUST be able to remove members from the team.
- **FR-021**: A **Team Captain** MUST be able to transfer captaincy to another team member.

#### Tournament Registration

- **FR-022**: A **Team Captain** MUST be able to register their team in a tournament, provided: the tournament is in "Open" status, the team's videogame matches the tournament's videogame, the maximum team count has not been reached, and the team currently has at least the tournament's configured minimum number of members.
- **FR-023**: The system MUST prevent duplicate registration of the same team in the same tournament.
- **FR-024**: A **Team Captain** MUST be able to withdraw their team's registration while the tournament status is "Open".

#### Scoring System

- **FR-029**: An **Organizer** MUST select a scoring system when creating a tournament. The available built-in options are:
  - **Standard**: 3 points for a win, 1 point for a draw, 0 points for a loss.
  - **Winner Takes All (WTA)**: 3 points for a win, 0 points for a draw or a loss.
  - **Custom**: the Organizer defines the exact point values for win, draw, and loss.
- **FR-030**: When **Custom** scoring is selected, the Organizer MUST provide non-negative integer values for win points, draw points, and loss points, with win ≥ draw ≥ loss.
- **FR-031**: The system MUST calculate and maintain the standings table strictly according to the tournament's configured scoring system. Standings must track, per team: total points, matches played, matches won, matches drawn, and matches lost. Teams are ranked by total points in descending order. When two or more teams are tied on total points, they share the same ordinal position using **standard competition ranking** (e.g., two teams tied on 6 points both receive rank 3, and the next team receives rank 5 — not rank 4); no secondary tiebreaker is applied.
- **FR-032**: The scoring system of a tournament MUST NOT be changeable once its status advances to "In Progress".
- **FR-033**: The scoring system logic MUST be implemented so that new scoring systems can be added to the platform without modifying any existing scoring or standings business logic (open for extension, closed for modification).
- **FR-034**: *(Reserved — removed: Single Elimination is out of scope; all tournaments use the league format.)*

#### Match & Results Management

- **FR-025**: An **Organizer** MUST be able to record a match result for their tournament by providing: home team, away team, score (goals or points per team as non-negative integers), and the date and time the match took place.
- **FR-026**: Recording a match result MUST automatically recalculate and update the standings table using the tournament's configured scoring system. A win is determined when one team's score exceeds the other's; equal scores constitute a draw.
- **FR-027**: An **Organizer** MUST be able to correct a previously recorded match result (score and/or date/time) as long as the tournament is not yet "Completed"; any correction MUST trigger a full standings recalculation.
- **FR-028**: The system MUST display updated standings and match results to all users within 5 seconds of a result being recorded or corrected, without requiring a full page reload. Updates MUST be pushed to connected clients via **SignalR** (ASP.NET Core WebSocket hub); the Angular frontend MUST subscribe to the relevant hub and update the standings and match list in place upon receiving a push event. The following frontend UX requirements apply to real-time updates:
  - **In-place refresh**: When the frontend receives a real-time update, it MUST automatically refresh the displayed standings and match list without reloading the page.
  - **Visual update indicator**: Upon receiving an update, the frontend MUST display a brief visual indication (e.g., a row highlight or transient notification) to signal to the user that the displayed data has changed.
  - **Connection loss warning**: If the real-time connection is interrupted, the frontend MUST inform the user that the displayed data may be outdated until the connection is restored.
  - **Automatic reconnection**: The SignalR client MUST attempt automatic reconnection after a connection loss, without requiring any user action.
- **FR-035**: The system MUST prevent recording a match where the same two teams (regardless of home/away order) already have a result recorded for the same date and time within the same tournament.

#### Error Handling

- **FR-040**: The backend MUST implement a **centralized exception-handling mechanism** (e.g., a global middleware or exception filter) that intercepts all unhandled exceptions before they reach the HTTP response. No individual controller or service method may handle exceptions in an ad-hoc way that bypasses this mechanism.
- **FR-041**: **Validation errors** (malformed input, constraint violations, missing required fields) MUST return HTTP **400 Bad Request** with a structured response body that identifies the field(s) in error and provides a human-readable description of each violated rule. The response MUST NOT expose stack traces or internal implementation details.
- **FR-042**: **Business rule violations** (e.g., duplicate registration, insufficient team members, forbidden state transition) MUST return the semantically appropriate HTTP status code — **409 Conflict** for uniqueness/duplicate violations, **422 Unprocessable Entity** for domain rule violations, **403 Forbidden** for authorization failures, **404 Not Found** for missing resources — each with a descriptive error message in the response body.
- **FR-043**: **Unhandled exceptions** (unexpected server-side errors) MUST return HTTP **500 Internal Server Error** with a generic error message. Internal details (stack traces, exception messages, database errors) MUST NOT be included in the response body in any environment.

#### Testing Requirements

The following four features are explicitly required to have **both automated unit tests and documented manual test cases**:

- **TR-001 — Player Account Registration** (FR-001, FR-038): Unit tests MUST cover all field validation rules (username uniqueness, email format and uniqueness, password rules, required fields). Manual test cases MUST cover the complete registration flow from the user interface, including validation feedback and successful account creation.
- **TR-002 — Tournament Creation** (FR-010, FR-012, FR-029, FR-030): Unit tests MUST cover field validation (future start date, end date after start date, max teams ≥ 2, min members ≥ 1, scoring system configuration and Custom scoring constraints). Manual test cases MUST cover the full creation flow from the Organizer interface, including Draft visibility behavior and state transitions.
- **TR-003 — Tournament Registration** (FR-022, FR-023, FR-024, FR-039): Unit tests MUST cover all eligibility validations (tournament status, videogame match, max team slots, minimum members, duplicate registration). Manual test cases MUST cover the Captain registration and withdrawal flows from the user interface, including all rejection scenarios.
- **TR-004 — Match Result Recording** (FR-025, FR-026, FR-027, FR-035): Unit tests MUST cover score recording, outcome derivation (win/draw/loss from scores), standings recalculation for each built-in scoring system, result correction triggering recalculation, and duplicate match detection. Manual test cases MUST cover the Organizer result entry flow from the user interface, including standings updates and error feedback.

### Key Entities

- **User**: Abstract representation of any registered account. Each User is either a Player or an Organizer; both share email and password credentials but have distinct registration flows and profile attributes.
- **Player**: A User who participates competitively. Holds: unique username, real name, unique email, password, and main videogame. Automatically assumes the Team Captain role for any team they create.
- **Organizer**: A User who manages tournaments. Identified by a unique organization name (3–60 characters) and email address. Registered through a dedicated flow; cannot simultaneously hold the Player role.
- **Team**: A group of Players organized to compete in a specific videogame. Has a name (unique within the videogame), a description, and exactly one Captain at all times.
- **Videogame**: A catalog entry representing a specific game title used to match teams with tournaments. The catalog is **fixed and seeded at application startup**; no runtime additions or removals are supported through any user interface or API.
- **Tournament**: A league competition for a specific videogame, owned by one Organizer. Attributes include name, description, start date, estimated end date, maximum teams (≥ 2), minimum members per team (default 5), current status (Draft / Open / In Progress / Completed), and the associated scoring system.
- **TournamentRegistration**: The formal enrollment of a Team in a Tournament. Links team, tournament, registration timestamp, and status.
- **Match**: A recorded contest between a home team and an away team within a tournament. Captures the score (goals/points per team as non-negative integers) and the date/time the match took place.
- **Standing**: The computed statistics for a team within a tournament at any given moment, derived from all recorded match results and the tournament's scoring system. Includes: position (rank), total points, matches played, matches won, matches drawn, and matches lost. Teams with equal total points share the same rank; no secondary tiebreaker is applied.
- **ScoringSystem**: A configuration entity that defines the point values awarded for a win, a draw, and a loss. Built-in variants are Standard and Winner Takes All; Custom is fully user-defined by the Organizer.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new user can complete registration and log in within 2 minutes.
- **SC-002**: An Organizer can create and advance a tournament to "Open" status within 5 minutes.
- **SC-003**: A Team Captain can register their team in an eligible tournament within 3 minutes.
- **SC-004**: Match result updates become visible to all users within 5 seconds of the Organizer recording them, without a manual page refresh.
- **SC-005**: Role-based access control is enforced with 100% accuracy — no operation is performed by a user who lacks the required role.
- **SC-006**: All unauthenticated requests to protected resources receive HTTP 401, and all role-insufficient requests receive HTTP 403, in 100% of cases.
- **SC-007**: All functional requirements (FR-001 through FR-039, excluding reserved entries) have at least one automated unit test verifying their core behavior. Features TR-001 through TR-004 additionally have documented manual test cases covering their full user-facing flows.
- **SC-008**: The standings table always reflects the complete and correct accumulation of all recorded match results using the tournament's scoring system — no result is omitted and no correction is ignored.

---

## Assumptions

- **Organizer registration**: Organizers self-register through a dedicated registration flow and are immediately granted the Organizer role upon successful validation. No Admin approval is required. A single account cannot hold both the Player and Organizer roles simultaneously.
- **Team captaincy auto-assignment**: The player who creates a team automatically becomes its Captain. No separate Admin action is needed.
- **Single team per game**: A player may join multiple teams across different videogames but may only be an active member of one team per videogame.
- **Tournament format**: All tournaments use the **League (Round Robin)** format — every participating team plays against every other team. There is no bracket or elimination structure. Final standings are determined by accumulated points as calculated by the tournament's configured scoring system. Single and double elimination formats are out of scope.
- **Invitation model**: Team Captains invite players by username or email. Invitations expire after a reasonable period (default: 7 days) if not accepted.
- **Real-time updates**: Near-real-time result visibility (SC-004) is required. Updates are delivered via **SignalR** (ASP.NET Core WebSocket hub); the Angular frontend subscribes to tournament-scoped hub events and updates standings and match results in place without a full page reload.
- **Tournament visibility**: Tournaments in "Open", "In Progress", or "Completed" status are publicly accessible without authentication. Tournaments in "Draft" status are only visible to their owning Organizer.
- **Admin bootstrapping**: At least one Admin account exists at system startup (seeded or configured via environment). Admin accounts cannot be created through the registration flow.
- **Player username immutability**: A Player's username and main videogame cannot be changed after registration. These are identity-establishing fields used for team invitations and videogame matching.
- **Team name uniqueness scope**: Team name uniqueness is enforced per videogame, not globally. Two teams in different videogames may share the same name.
- **Standings visibility**: The standings table is a feature for authenticated users. Anonymous visitors can see tournament listings and match history, but not the standings table.
- **Score format**: Match scores are recorded as non-negative integer values per team (e.g., home 2 – away 1). The system derives the outcome (win/draw/loss) from comparing the two scores; the Organizer does not designate a winner explicitly. Equal scores constitute a draw.
- **Videogame catalog**: The list of available videogames is fixed and seeded at application startup (e.g., via a database migration seed or startup initializer). No API endpoint or UI flow exists to add, edit, or remove videogames at runtime. Developers extend the catalog by updating the seed data.
- **Scoring system extensibility**: The platform uses a strategy-like pattern for scoring so future scoring systems (e.g., FIDE-style chess scoring) can be introduced without changing any existing tournament, match, or standings logic.
- **Technical stack**: The implementation must use .NET 10 + EF Core (backend), Angular 21 (frontend), SQL Server (database), JWT for authentication, and MSTest + Moq for testing. Architecture must enforce strict layer separation: controllers contain no business logic, repositories contain only data access, business logic has no dependency on infrastructure, and every dependency is expressed through an interface.
