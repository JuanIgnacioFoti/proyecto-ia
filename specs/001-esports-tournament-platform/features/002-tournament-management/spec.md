# Feature Specification: Tournament Creation and Management

**Feature**: `002-tournament-management`  
**Parent**: `001-esports-tournament-platform`  
**Created**: 2026-04-24  
**Status**: Final  
**Priority**: P2

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create a Tournament (Priority: P1)

An Organizer creates a new league tournament by providing: name, videogame, description, a start date strictly in the future, an estimated end date after the start date, a maximum number of teams (at least 2), a minimum number of members per team (at least 1, default 5), and a scoring system. The tournament is created in **Draft** status and is visible only to its owning Organizer.

**Why this priority**: Tournament creation is the Organizer's core action. All downstream features — registration, match recording, and standings — depend on a tournament existing.

**Independent Test**: Log in as an Organizer; create a tournament with all valid fields; verify it appears in the Organizer's own list with status "Draft". Log in as a different user (Player or other Organizer); verify the Draft tournament does not appear in their listing.

**Acceptance Scenarios**:

1. **Given** an authenticated Organizer, **When** they create a tournament providing name, videogame, description, a future start date, an estimated end date after the start date, a maximum team count of at least 2, a minimum members per team of at least 1, and a scoring system, **Then** the tournament is created with status "Draft" and is only visible to that Organizer.
2. **Given** an Organizer attempting to create a tournament with a start date in the past, **When** they submit the form, **Then** the system rejects the request with a validation error.
3. **Given** an Organizer attempting to create a tournament with an estimated end date before or equal to the start date, **When** they submit the form, **Then** the system rejects the request with a validation error.
4. **Given** an Organizer attempting to create a tournament with maximum teams fewer than 2 or minimum members per team fewer than 1, **When** they submit the form, **Then** the system rejects the request with a validation error.
5. **Given** an unauthenticated visitor, **When** they attempt to create a tournament, **Then** the system returns HTTP 401.

---

### User Story 2 - Advance Tournament Lifecycle (Priority: P1)

The Organizer manually advances a tournament through its lifecycle: Draft → Open → In Progress → Completed. Transitions are forward-only; states can be skipped but never reversed. Each state controls what actions are allowed.

**Why this priority**: Lifecycle state drives every gate in the platform — visibility, registration eligibility, result recording, and finality. Without it, no other controlled action is possible.

**Independent Test**: Create a tournament (Draft); advance to Open; verify public visibility; advance to In Progress; verify registration is closed; advance to Completed; attempt to advance again and verify it is rejected; attempt to revert to In Progress and verify that is also rejected.

**Acceptance Scenarios**:

1. **Given** a tournament in any status before "Completed", **When** the Organizer advances its status, **Then** the status moves to the next or any later state in the sequence (Draft → Open → In Progress → Completed); the status can never revert to a previous state.
2. **Given** a tournament advanced to "Open" status, **When** any visitor views the tournament list, **Then** the tournament appears publicly with status "Open".
3. **Given** a tournament in "Draft" status, **When** any user other than the owning Organizer attempts to view it, **Then** it does not appear in tournament listings or detail views.
4. **Given** a tournament in "Completed" status, **When** the Organizer attempts to advance it further, **Then** the system rejects the attempt.
5. **Given** an authenticated Player or a different Organizer, **When** they attempt to advance another Organizer's tournament, **Then** the system returns HTTP 403.

---

### User Story 3 - Edit Tournament Details (Priority: P2)

The Organizer updates editable fields of a tournament (name, description, dates, team limits, minimum members, scoring system) while it is in Draft or Open status. Once the tournament is In Progress, the scoring system is locked.

**Why this priority**: Editing is a natural complement to creation and lets the Organizer refine details before the tournament goes live or starts.

**Independent Test**: Create a tournament in Draft; update the name and max teams; verify changes are persisted. Advance to In Progress; attempt to change the scoring system; verify the system rejects the change. Attempt to change the name; verify the change is accepted.

**Acceptance Scenarios**:

1. **Given** a tournament in "Draft" or "Open" status, **When** the Organizer updates editable fields (name, description, dates, team limits, minimum members per team), **Then** the changes are persisted.
2. **Given** a tournament in "In Progress" or later status, **When** the Organizer attempts to change the scoring system, **Then** the system rejects the change.
3. **Given** an authenticated Player or a different Organizer, **When** they attempt to modify another Organizer's tournament, **Then** the system returns HTTP 403.

---

### User Story 4 - Configure Scoring System (Priority: P2)

When creating a tournament the Organizer selects one of three scoring systems: **Standard** (3/1/0 for win/draw/loss), **Winner Takes All** (3/0/0), or **Custom** (Organizer-defined non-negative integers where win ≥ draw ≥ loss).

**Why this priority**: The scoring system determines how standings are calculated throughout the tournament's life. It must be configured at creation time and validated before any matches are recorded.

**Independent Test**: Create a tournament with Standard scoring; verify standings calculation uses 3/1/0. Create a tournament with Custom scoring using win=2, draw=1, loss=0; verify standings reflect those values. Attempt to create a Custom tournament with draw > win; verify the system rejects the configuration.

**Acceptance Scenarios**:

1. **Given** an Organizer creating a tournament, **When** they select Standard scoring, **Then** the tournament uses 3 points for a win, 1 for a draw, and 0 for a loss.
2. **Given** an Organizer creating a tournament, **When** they select Winner Takes All, **Then** the tournament uses 3 points for a win and 0 for a draw or loss.
3. **Given** an Organizer creating a tournament, **When** they select Custom scoring and provide non-negative integer values where win ≥ draw ≥ loss, **Then** the tournament uses those exact values.
4. **Given** an Organizer selecting Custom scoring, **When** they provide values where draw > win, loss > draw, or any value is negative, **Then** the system rejects the configuration with a validation error.
5. **Given** a tournament in "In Progress" or later status, **When** any user attempts to change the scoring system, **Then** the system rejects the change.

---

### User Story 5 - Suspended Tournament State (Priority: P3)

When an Admin suspends an Organizer, all that Organizer's non-Completed tournaments enter a Suspended state. No registrations, status advances, or result changes are permitted while Suspended. Publicly visible tournaments (Open, In Progress, Completed before suspension) remain listed with a "Suspended" badge. On Organizer reinstatement, tournaments return to their pre-suspension states.

**Why this priority**: Suspension is an Admin governance action that protects platform integrity. It is a lower-priority edge case but must be specified to ensure the lifecycle model is complete.

**Independent Test**: Suspend an Organizer who has an Open tournament; verify the tournament appears in public listings with a "Suspended" badge; attempt to register a team in the suspended tournament and verify it is rejected; reinstate the Organizer; verify the tournament returns to "Open" and registrations are accepted again.

**Acceptance Scenarios**:

1. **Given** an Organizer whose account is suspended by an Admin, **When** any user views the tournament list, **Then** all of that Organizer's tournaments that were publicly visible (Open, In Progress, or Completed before suspension) appear with a "Suspended" badge and do NOT disappear from listings.
2. **Given** a tournament in the Suspended state, **When** any user attempts to register, advance status, or record a result, **Then** the system rejects the attempt.
3. **Given** a tournament in Suspended state that was in "In Progress" before suspension, **When** the Admin reinstates the Organizer, **Then** the tournament returns to "In Progress".
4. **Given** a tournament in Suspended state that was in "Draft" before suspension (i.e., visible only to the Organizer), **When** the Admin reinstates the Organizer, **Then** the tournament returns to "Draft" and is not visible to other users.

---

### Edge Cases

- What happens when the Organizer tries to advance a tournament that has no registered teams to "In Progress"? → The system allows the transition; ensuring adequate participation is the Organizer's responsibility.
- What happens when the Organizer provides a start date in the past when creating a tournament? → The system rejects the creation with a validation error.
- What happens if an Organizer account is suspended while one of their tournaments is already Completed? → Completed tournaments are unaffected; they remain Completed.
- What happens if a Custom scoring system is configured with values that make standings ambiguous (e.g., win < draw)? → The system validates that win ≥ draw ≥ loss and all values are non-negative integers at creation time; invalid configurations are rejected.

---

## Requirements *(mandatory)*

### Functional Requirements

#### Tournament Creation & Editing

- **FR-010**: An **Organizer** MUST be able to create a league (round-robin) tournament specifying: name, videogame, description, start date (must be strictly in the future at the moment of creation), estimated end date (must be after the start date), maximum number of teams (minimum 2), minimum number of members per team (minimum 1, default 5), and a scoring system.
- **FR-011**: An **Organizer** MUST only be able to create, edit, and delete tournaments they own.
- **FR-015**: Unauthenticated visitors MUST be able to view tournament listings (Open, In Progress, Completed only) and tournament details including the registered team list and match history (home team, away team, score, date/time). Tournaments in "Draft" status MUST NOT appear to any user other than the owning Organizer.

#### Tournament Lifecycle

- **FR-012**: A tournament has five lifecycle states: **Draft**, **Open**, **In Progress**, **Completed**, and **Suspended**. The following rules apply:
  - **Draft**: Visible only to its owning Organizer. Team registration not permitted.
  - **Open**: Publicly visible. Teams may register.
  - **In Progress**: No new team registrations accepted. Match results can be recorded.
  - **Completed**: Closed. No further result changes permitted.
  - **Suspended**: Entered automatically when the owning Organizer's account is suspended. No registrations, status advances, or result changes permitted. Publicly visible tournaments remain in public listings with a "Suspended" badge. Returns to pre-suspension state on Organizer reinstatement.
  - Normal transitions (Draft → Open → In Progress → Completed) are **unidirectional and forward-only**. The Organizer may advance the status to the next state or skip to any later state, but can never revert. The Suspended state is set and cleared exclusively by Admin action.
- **FR-014**: The system MUST prevent registration of new teams once a tournament's status is "In Progress" or later.

#### Scoring System

- **FR-029**: An **Organizer** MUST select a scoring system when creating a tournament. Built-in options:
  - **Standard**: 3 points for a win, 1 for a draw, 0 for a loss.
  - **Winner Takes All (WTA)**: 3 points for a win, 0 for a draw or a loss.
  - **Custom**: the Organizer defines exact point values for win, draw, and loss.
- **FR-030**: When **Custom** scoring is selected, the Organizer MUST provide non-negative integer values for win, draw, and loss points, with win ≥ draw ≥ loss.
- **FR-031**: The system MUST calculate and maintain the standings table strictly according to the tournament's configured scoring system. Teams are ranked by total points in descending order. Tied teams share the same ordinal position using **standard competition ranking** (e.g., two teams tied on 6 points both receive rank 3, and the next team receives rank 5); no secondary tiebreaker is applied.
- **FR-032**: The scoring system of a tournament MUST NOT be changeable once its status advances to "In Progress".
- **FR-033**: The scoring system logic MUST be implemented so that new scoring systems can be added without modifying any existing scoring or standings business logic (open for extension, closed for modification).

### Key Entities

- **Tournament**: A league competition for a specific videogame, owned by one Organizer. Attributes: name, description, start date, estimated end date, maximum teams (≥ 2), minimum members per team (default 5), current status (Draft / Open / In Progress / Completed / Suspended), and the associated scoring system.
- **ScoringSystem**: A configuration entity that defines the point values for win, draw, and loss. Built-in variants: Standard and Winner Takes All. Custom is fully user-defined by the Organizer.
- **Videogame**: A catalog entry representing a specific game title. The catalog is fixed and seeded at startup; no runtime additions are supported.

---

## Success Criteria *(mandatory)*

- **SC-002**: An Organizer can create and advance a tournament to "Open" status within 5 minutes.
- **SC-005**: Role-based access control is enforced with 100% accuracy — no operation is performed by a user who lacks the required role.

---

## Testing Requirements

- **TR-002 — Tournament Creation** (FR-010, FR-012, FR-029, FR-030): Unit tests MUST cover field validation (future start date, end date after start date, max teams ≥ 2, min members ≥ 1, scoring system configuration and Custom scoring constraints). Manual test cases MUST cover the full creation flow from the Organizer interface, including Draft visibility behavior and state transitions.

---

## Assumptions

- **Tournament format**: All tournaments use the **League (Round Robin)** format — every participating team plays against every other team. Single and double elimination formats are out of scope.
- **Admin bootstrapping**: At least one Admin account exists at system startup. Admin accounts cannot be created through the registration flow.
