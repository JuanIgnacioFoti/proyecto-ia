# Feature Specification: Match Result Recording

**Feature**: `006-match-result-recording`  
**Parent**: `001-esports-tournament-platform`  
**Created**: 2026-04-24  
**Status**: Final  
**Priority**: P6

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record a Match Result (Priority: P1)

The Organizer records the outcome of a match by specifying the home team, away team, the score (goals or points per team as non-negative integers), and the date and time the match took place. The system automatically determines the winner (or draw) from the scores and recalculates the standings using the tournament's configured scoring system.

**Why this priority**: Result recording is what drives the league standings. Without recorded results, no competitive progression can be tracked and the platform's core purpose cannot be demonstrated.

**Independent Test**: Create a tournament with at least two registered teams and advance it to In Progress; record a match as the Organizer; verify the match appears in the match history; verify the standings table reflects the correct accumulated points per the configured scoring system. Attempt to record the same result as a Player; verify HTTP 403.

**Acceptance Scenarios**:

1. **Given** a tournament "In Progress" and an authenticated Organizer who owns it, **When** they record a match providing home team, away team, score (goals/points per team as non-negative integers), and date/time, **Then** the result is persisted and the standings table is automatically recalculated using the tournament's scoring system.
2. **Given** a match where one team's score exceeds the other's, **When** the result is recorded, **Then** the winning team receives the win points and the losing team receives the loss points per the scoring system.
3. **Given** a match where both teams have the same score, **When** the result is recorded, **Then** both teams receive the draw points per the scoring system.
4. **Given** an authenticated Player or an Organizer who does not own the tournament, **When** they attempt to record a result, **Then** the system returns HTTP 403.
5. **Given** a tournament with status "Completed" or "Draft", **When** any user attempts to record a new match result, **Then** the system rejects the request.

---

### User Story 2 - Prevent Duplicate Match Records (Priority: P1)

The system prevents recording a match between the same two teams (regardless of home/away order) at the same date and time that already has an existing result in that tournament.

**Why this priority**: Duplicate results would corrupt the standings. This is a data integrity gate that must be enforced at the same time result recording is implemented.

**Independent Test**: Record a match between Team A (home) and Team B (away) at a given date/time; attempt to record another match with Team B as home and Team A as away at the same date/time; verify the system rejects the duplicate. Change the date/time to a different value; verify the second record is accepted.

**Acceptance Scenarios**:

1. **Given** the same two teams already have a match recorded with the same date and time in the same tournament, **When** the Organizer attempts to record another match with those two teams at that same date/time (regardless of home/away order), **Then** the system rejects the request with a validation error.
2. **Given** the same two teams with an existing match at a given date/time, **When** the Organizer records another match at a different date/time, **Then** the new record is accepted.

---

### User Story 3 - Correct a Match Result (Priority: P2)

The Organizer can correct a previously recorded match result (score and/or date/time) as long as the tournament is not yet Completed. Any correction triggers a full recalculation of the standings table.

**Why this priority**: Mistakes happen. The ability to correct results before the tournament is finalized prevents permanent data errors and maintains standing accuracy.

**Independent Test**: Record a match result; verify the standings; correct the score; verify the standings are fully recalculated to reflect the corrected values. Advance the tournament to Completed; attempt another correction; verify the system rejects it.

**Acceptance Scenarios**:

1. **Given** a previously recorded match result, **When** the Organizer corrects the score or date/time (before the tournament is Completed), **Then** the correction is persisted and the standings table is fully recalculated to reflect the change.
2. **Given** a tournament with status "Completed", **When** the Organizer attempts to correct any match result, **Then** the system rejects the request.
3. **Given** a corrected match that would create a duplicate (same two teams, same date/time as another existing match), **When** the Organizer submits the correction, **Then** the system rejects it with a validation error.

---

### Edge Cases

- What happens when the Organizer records a match between the same two teams at the same date and time as an existing result? → The system rejects the request with a validation error; the existing result is unaffected.
- What happens when the Organizer records a match score where both teams have the same number of goals/points? → The result is a draw; the scoring system distributes draw points to both teams.
- What happens when a match result is corrected and the outcome changes from a win to a draw (or vice versa)? → The standings are fully recalculated from scratch using all recorded results; the previous points are not carried over.
- What happens when the Organizer attempts to record a result in a Suspended tournament? → The system rejects the request; no result changes are permitted while a tournament is Suspended.
- What happens if the home and away team are the same? → The system rejects the request with a validation error; a team cannot play against itself.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-025**: An **Organizer** MUST be able to record a match result for their tournament by providing: home team, away team, score (goals or points per team as non-negative integers), and the date and time the match took place.
- **FR-026**: Recording a match result MUST automatically recalculate and update the standings table using the tournament's configured scoring system. A win is determined when one team's score exceeds the other's; equal scores constitute a draw.
- **FR-027**: An **Organizer** MUST be able to correct a previously recorded match result (score and/or date/time) as long as the tournament is not yet "Completed"; any correction MUST trigger a full standings recalculation.
- **FR-028**: The system MUST display updated standings and match results to all users within 5 seconds of a result being recorded or corrected, without requiring a full page reload. Updates MUST be pushed to connected clients via **SignalR**.
- **FR-035**: The system MUST prevent recording a match where the same two teams (regardless of home/away order) already have a result recorded for the same date and time within the same tournament.

### Key Entities

- **Match**: A recorded contest between a home team and an away team within a tournament. Captures the score (goals/points per team as non-negative integers) and the date/time the match took place.
- **Standing**: The computed statistics for a team within a tournament, derived from all recorded match results and the tournament's scoring system. Includes: position, total points, matches played, matches won, matches drawn, and matches lost.
- **ScoringSystem**: Defines the point values for win, draw, and loss. Determines how standings are recalculated after each result.

---

## Success Criteria *(mandatory)*

- **SC-004**: Match result updates become visible to all users within 5 seconds of the Organizer recording them, without a manual page refresh.
- **SC-008**: The standings table always reflects the complete and correct accumulation of all recorded match results using the tournament's scoring system — no result is omitted and no correction is ignored.

---

## Testing Requirements

- **TR-004 — Match Result Recording** (FR-025, FR-026, FR-027, FR-035): Unit tests MUST cover score recording, outcome derivation (win/draw/loss from scores), standings recalculation for each built-in scoring system, result correction triggering recalculation, and duplicate match detection. Manual test cases MUST cover the Organizer result entry flow from the user interface, including standings updates and error feedback.

---

## Assumptions

- The outcome of a match (win/draw/loss) is determined solely by comparing the two scores. No overtime or penalty system exists.
- Standings are always recalculated from the full set of recorded results; there is no incremental update that could leave standings in an inconsistent state after a correction.
