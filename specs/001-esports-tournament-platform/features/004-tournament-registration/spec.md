# Feature Specification: Tournament Registration

**Feature**: `004-tournament-registration`  
**Parent**: `001-esports-tournament-platform`  
**Created**: 2026-04-24  
**Status**: Final  
**Priority**: P4

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register a Team in a Tournament (Priority: P1)

A Team Captain registers their team in an open tournament. The system validates that: the tournament is in "Open" status, the team's videogame matches the tournament's videogame, the tournament has not reached its maximum team count, and the team currently has at least the tournament's configured minimum number of members.

**Why this priority**: Registration is the link that connects teams to tournaments. Without it, no competitive structure exists, no matches can be scheduled, and standings cannot be produced.

**Independent Test**: Create a team for game X with the required minimum members; create a tournament for game X in Open status; have the Captain register the team; verify the team appears in the tournament's participant list. Attempt to register the same team again; verify duplicate is rejected. Attempt to register a team for game Y in the game X tournament; verify mismatch is rejected.

**Acceptance Scenarios**:

1. **Given** a Team Captain and an open tournament for the same videogame as their team, where the team meets all eligibility criteria, **When** the Captain submits a registration, **Then** the team is added to the tournament's participant list.
2. **Given** a Team Captain attempting to register in a tournament for a different videogame, **When** the request is submitted, **Then** the system rejects it with a validation error.
3. **Given** a team already registered in a tournament, **When** the Captain attempts to register again in the same tournament, **Then** the system rejects the duplicate registration.
4. **Given** a tournament that has reached its maximum team count, **When** a new registration is attempted, **Then** the system rejects it.
5. **Given** a team that does not meet the tournament's minimum member count, **When** the Captain attempts to register, **Then** the system rejects it with a validation error stating the minimum members required.
6. **Given** a tournament in "Draft", "In Progress", or "Completed" status, **When** a Captain attempts to register, **Then** the system rejects the request.

---

### User Story 2 - Withdraw a Team Registration (Priority: P2)

A Team Captain can withdraw their team's registration from a tournament as long as the tournament is still in "Open" status. Once the tournament moves to "In Progress" or later, withdrawal is no longer allowed.

**Why this priority**: Withdrawal allows Captains to correct registration mistakes or respond to roster changes before the tournament begins. It is the natural counterpart to registration.

**Independent Test**: Register a team in an Open tournament; have the Captain withdraw; verify the team is removed from the participant list and the slot count decreases. Advance the tournament to In Progress; attempt a withdrawal; verify it is rejected.

**Acceptance Scenarios**:

1. **Given** a Team Captain who has registered their team, **When** the tournament is still "Open" and they withdraw, **Then** the team is removed from the participant list and the slot becomes available again.
2. **Given** a tournament with status "In Progress" or "Completed", **When** any withdrawal is attempted, **Then** the system rejects it.
3. **Given** a Player who is not the Team Captain, **When** they attempt to withdraw the team's registration, **Then** the system returns HTTP 403.

---

### User Story 3 - View Tournament Participants (Priority: P3)

Any visitor can view the list of teams registered in a tournament that is in Open, In Progress, or Completed status. The participant list is not accessible for Draft tournaments.

**Why this priority**: Public visibility of participants supports spectators and potential registrants in deciding to follow or join a tournament. It is a read-only view that can be delivered independently.

**Independent Test**: Register two teams in an Open tournament; access the tournament's participant list as an unauthenticated visitor; verify both teams are visible. Access the participant list of a Draft tournament as a non-owner; verify it is not shown.

**Acceptance Scenarios**:

1. **Given** any visitor, **When** they view an Open, In Progress, or Completed tournament, **Then** the registered team list is visible.
2. **Given** a tournament in "Draft" status, **When** a non-owner visitor accesses it, **Then** the participant list is not displayed.

---

### Edge Cases

- What happens if a team disbands after registering for a tournament that has not yet started? → The registration is automatically withdrawn and the slot becomes available.
- What happens if a Player who is not a Captain attempts to register? → The system returns HTTP 403; only Team Captains may register teams.
- What happens when a Suspended tournament receives a registration attempt? → The system rejects the attempt; no registrations or withdrawals are permitted while a tournament is Suspended.
- What happens if a team had enough members at registration time but loses members after registering? → The team remains registered; minimum member count is validated only at the moment of registration.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-022**: A **Team Captain** MUST be able to register their team in a tournament, provided: the tournament is in "Open" status, the team's videogame matches the tournament's videogame, the maximum team count has not been reached, and the team currently has at least the tournament's configured minimum number of members.
- **FR-023**: The system MUST prevent duplicate registration of the same team in the same tournament.
- **FR-024**: A **Team Captain** MUST be able to withdraw their team's registration while the tournament status is "Open".
- **FR-014**: The system MUST prevent registration of new teams once a tournament's status is "In Progress" or later.
- **FR-015** *(partial)*: Unauthenticated visitors MUST be able to view the registered team list for Open, In Progress, and Completed tournaments.

### Key Entities

- **TournamentRegistration**: The formal enrollment of a Team in a Tournament. Links team, tournament, registration timestamp, and status.
- **Team**: Must have the same videogame as the tournament and meet the minimum member count at registration time.
- **Tournament**: Must be in "Open" status and below its maximum team count at registration time.

---

## Success Criteria *(mandatory)*

- **SC-003**: A Team Captain can register their team in an eligible tournament within 3 minutes.
- **SC-005**: Role-based access control is enforced with 100% accuracy — no operation is performed by a user who lacks the required role.

---

## Testing Requirements

- **TR-003 — Tournament Registration** (FR-022, FR-023, FR-024, FR-039): Unit tests MUST cover all eligibility validations (tournament status, videogame match, max team slots, minimum members, duplicate registration). Manual test cases MUST cover the Captain registration and withdrawal flows from the user interface, including all rejection scenarios.

---

## Assumptions

- Minimum member count is validated only at the moment of registration. Teams that lose members after registering remain registered.
- Only Team Captains may submit or withdraw a registration; regular Players and Organizers cannot.
