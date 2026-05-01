# Feature Specification: Team Formation and Player Management

**Feature**: `003-team-formation`  
**Parent**: `001-esports-tournament-platform`  
**Created**: 2026-04-24  
**Status**: Final  
**Priority**: P3

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create a Team (Priority: P1)

A Player creates a team for a specific videogame (which must match their own main videogame) by providing a unique name within that videogame and a description. The Player automatically becomes the team's **Captain**. A Player can only be Captain of one team per videogame.

**Why this priority**: Team creation is the prerequisite for every other team-related action. Without it, no roster can be built and no tournament can be entered.

**Independent Test**: Log in as a Player; create a team for their main videogame; verify the team is created and the Player is listed as Captain. Attempt to create a second team for the same videogame; verify the system rejects the attempt.

**Acceptance Scenarios**:

1. **Given** an authenticated Player, **When** they create a team providing a name unique within that videogame, a description, and the main videogame, **Then** the team is created with that Player as Captain.
2. **Given** a Player who is already a Captain of a team for a given videogame, **When** they attempt to create a second team for the same videogame, **Then** the system rejects the attempt.
3. **Given** a Player attempting to create a team for a videogame that is not their own main videogame, **When** they submit the form, **Then** the system rejects the attempt.
4. **Given** a Player attempting to create a team with a name already used by another team for the same videogame, **When** they submit the form, **Then** the system rejects the attempt with a validation error.
5. **Given** an unauthenticated visitor, **When** they attempt to create a team, **Then** the system returns HTTP 401.

---

### User Story 2 - Invite Players to a Team (Priority: P2)

A Team Captain invites registered Players to join their team by specifying the target Player's username or email address. The target Player must not already be an active member of any team for that videogame. Invitations expire after 7 days if not accepted.

**Why this priority**: Building a roster through invitations is the primary way teams reach the minimum size required for tournament registration.

**Independent Test**: Create two Player accounts for the same videogame; have one (Captain) invite the other by username; verify an invitation record is created. Have the invited Player accept; verify they appear in the team roster. Attempt to invite a Player who is already a member of a different team for that videogame; verify the system rejects the invitation.

**Acceptance Scenarios**:

1. **Given** a Team Captain, **When** they invite a Player by email or username who is not already an active member of a team for that videogame, **Then** the invitation is sent and the Player can accept or decline.
2. **Given** a Team Captain, **When** they invite a Player who is already an active member of another team for the same videogame, **Then** the system rejects the invitation.
3. **Given** a Player who received an invitation, **When** they accept, **Then** they are added to the team roster.
4. **Given** a Player who received an invitation, **When** they decline, **Then** the invitation is closed and the Player's team membership is unchanged.
5. **Given** an invitation that has not been accepted or declined within 7 days, **When** any user inspects it, **Then** it is marked expired and the Player cannot use it to join the team.

---

### User Story 3 - Manage Team Roster (Priority: P2)

A Team Captain can remove members from the team and transfer captaincy to another member. A Player can only be an active member of one team per videogame at any time.

**Why this priority**: Roster management keeps the team composition accurate throughout the tournament lifecycle and is necessary for handling player changes before registration.

**Independent Test**: Have a Captain remove a member; verify the member no longer appears in the roster and is free to join another team. Have a Captain transfer captaincy; verify the previous Captain is now a regular member and the new Captain can perform Captain-only actions.

**Acceptance Scenarios**:

1. **Given** a Team Captain, **When** they remove a Player from the team, **Then** the Player is no longer part of the team and is free to join another team for that videogame.
2. **Given** a Team Captain, **When** they transfer captaincy to another team member, **Then** that member becomes the new Captain with full Captain privileges, and the previous Captain becomes a regular member.
3. **Given** a Player who has accepted a team invitation, **When** another team for the same videogame tries to include them, **Then** the system rejects the attempt.
4. **Given** a Team Captain who is the sole member of their team and attempts to leave without transferring captaincy, **When** they submit the request, **Then** the team is disbanded.
5. **Given** a Team Captain with at least one other member, **When** they attempt to leave without first transferring captaincy, **Then** the system requires them to transfer captaincy before leaving.

---

### Edge Cases

- What happens when a Team Captain leaves their own team? → Captaincy must be transferred to another member before leaving. If no other members exist, the team is disbanded.
- What happens if a team disbands after registering for a tournament that has not yet started? → The registration is automatically withdrawn and the slot becomes available.
- What happens if a team had enough members at registration time but loses members before the tournament starts? → The team remains registered; minimum member count is validated only at the moment of registration.
- What happens when a Player is simultaneously invited by two teams for the same videogame? → Each invitation is pending independently. The Player may accept at most one; accepting the first automatically invalidates (expires or cancels) any other pending invitations for that same videogame.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-016**: A **Player** MUST be able to create a team by providing: a name (unique within the selected videogame), a description, and the main videogame. The creating Player automatically becomes the team's **Team Captain**. A Player MUST NOT be the Captain of more than one team per videogame.
- **FR-017**: A **Team Captain** MUST be able to invite registered Players to join their team by specifying the target Player's username or email address.
- **FR-018**: A **Player** MUST be able to accept or decline a team invitation.
- **FR-019**: A **Player** MUST NOT be a member of more than one team for the same videogame simultaneously.
- **FR-020**: A **Team Captain** MUST be able to remove members from the team.
- **FR-021**: A **Team Captain** MUST be able to transfer captaincy to another team member.

### Key Entities

- **Team**: A group of Players organized to compete in a specific videogame. Has a name (unique within the videogame), a description, and exactly one Captain at all times.
- **Player**: A registered user who participates competitively. Identified by a unique username. Can be a member of at most one team per videogame.
- **Videogame**: A catalog entry representing a specific game title. A team's videogame must match the Captain's main videogame at creation time.

---

## Success Criteria *(mandatory)*

- **SC-003**: A Team Captain can register their team in an eligible tournament within 3 minutes (this includes the team already existing; team creation itself should take under 2 minutes for a Captain).
- **SC-005**: Role-based access control is enforced with 100% accuracy — no operation is performed by a user who lacks the required role.

---

## Assumptions

- **Single team per game**: A player may join multiple teams across different videogames but may only be an active member of one team per videogame.
- **Team captaincy auto-assignment**: The player who creates a team automatically becomes its Captain. No separate Admin action is needed.
- **Invitation model**: Team Captains invite players by username or email. Invitations expire after 7 days if not accepted or declined.
