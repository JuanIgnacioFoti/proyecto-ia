# Feature Specification: Results and Standings Viewing

**Feature**: `005-results-and-standings`  
**Parent**: `001-esports-tournament-platform`  
**Created**: 2026-04-24  
**Status**: Final  
**Priority**: P5

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse Tournaments and Match History (Priority: P1)

Any visitor (unauthenticated or authenticated) can browse the list of public tournaments (Open, In Progress, Completed) and view a tournament's match history, including home team, away team, score, and date/time. No login is required for this read-only access.

**Why this priority**: Public browsing is the lowest barrier to platform engagement. It requires no authentication and can be demonstrated as soon as tournaments and match results exist in the system.

**Independent Test**: Without logging in, navigate to the tournament list; verify Open, In Progress, and Completed tournaments are visible. Open a tournament page; verify the registered team list and match history (home team, away team, score, date/time) are displayed. Verify Draft tournaments do not appear.

**Acceptance Scenarios**:

1. **Given** any visitor, **When** they navigate to the tournament list, **Then** they see all Open, In Progress, and Completed tournaments with their current status.
2. **Given** any visitor, **When** they open a tournament page, **Then** they see the list of registered teams and the match history with home team, away team, score, and date/time.
3. **Given** a tournament in "Draft" status, **When** any non-owner user navigates to the tournament list, **Then** the Draft tournament does not appear.

---

### User Story 2 - View Standings Table (Priority: P2)

An authenticated user can view the standings table for any tournament that is In Progress or Completed. The table shows, for each team: position (ranked by total points descending), team name, total points, matches played, matches won, matches drawn, and matches lost. Tied teams share the same position using standard competition ranking.

**Why this priority**: The standings table is the primary competitive output of a league tournament. It must be accurate at all times and accessible to any logged-in user.

**Independent Test**: Log in as a Player; open a tournament in In Progress status; verify the standings table displays all required columns with correct values. Tie two teams on equal points; verify both receive the same rank and the next team receives the rank that skips over the tied positions. Attempt to view the standings table as an unauthenticated visitor; verify it is not visible or returns HTTP 401.

**Acceptance Scenarios**:

1. **Given** an authenticated user, **When** they view a tournament in "In Progress" or "Completed" status, **Then** they see the standings table with, for each team: position, team name, total points, matches played, matches won, matches drawn, and matches lost.
2. **Given** two or more teams with equal total points in the standings, **When** their positions are displayed, **Then** those teams share the same ordinal position (e.g., two teams tied at 6 points both receive rank 3, and the next team receives rank 5); no secondary tiebreaker is applied.
3. **Given** an unauthenticated visitor, **When** they attempt to access the standings table, **Then** the table is not shown or the system returns HTTP 401.
4. **Given** a completed tournament, **When** an authenticated Player views it, **Then** the final standings and all match results are permanently accessible.

---

### User Story 3 - Real-Time Standings Updates (Priority: P3)

When an Organizer records or corrects a match result, all authenticated users currently viewing that tournament's page see the updated standings and match list automatically within 5 seconds — without reloading the page. The frontend displays a brief visual indicator when new data arrives and warns users if the real-time connection is lost.

**Why this priority**: Real-time updates are a key engagement feature that differentiates the platform from a static results page. They depend on match result recording already working.

**Independent Test**: Open a tournament page as a logged-in user; have the Organizer record a match result in another session; verify the standings and match list update within 5 seconds without a page reload; verify a brief visual highlight or notification appears. Disconnect the client; verify a connection-lost warning is shown. Reconnect; verify the data is up to date.

**Acceptance Scenarios**:

1. **Given** a match result that has just been recorded by the Organizer, **When** an authenticated user views the tournament page, **Then** the updated standings and match result appear within 5 seconds without a full page reload.
2. **Given** the frontend receives a real-time update, **When** the standings or match list changes, **Then** the page updates in place and displays a brief visual indication (e.g., a row highlight or transient notification) that the data has changed.
3. **Given** the real-time connection is interrupted, **When** the user is viewing a tournament page, **Then** the frontend displays a warning that the displayed data may be outdated.
4. **Given** a connection loss, **When** the connection is restored, **Then** the frontend automatically reconnects without requiring any user action.

---

### Edge Cases

- What happens when a user views the standings of a tournament with no matches recorded yet? → The standings table is displayed with all teams at 0 points, 0 wins, 0 draws, 0 losses, 0 played, all sharing rank 1.
- What happens when the standings are viewed for a Suspended tournament? → The standings and match history up to the point of suspension are displayed; no new data appears while suspended.
- What happens if the real-time connection fails permanently? → The user sees a persistent warning that data may be stale; they can manually reload the page to see the latest data.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-015**: Unauthenticated visitors MUST be able to view tournament listings (Open, In Progress, Completed only) and tournament details including the registered team list and match history (home team, away team, score, date/time). Tournaments in "Draft" status MUST NOT appear to any user other than the owning Organizer.
- **FR-039**: Any **authenticated** user MUST be able to view the standings table for any tournament in "In Progress" or "Completed" status. The standings table MUST display, for each team: position (ranked by points, descending), team name, total points, matches played, matches won, matches drawn, and matches lost.
- **FR-031** *(partial)*: Standings must track, per team: total points, matches played, matches won, matches drawn, and matches lost. Teams are ranked by total points in descending order. Tied teams share the same ordinal position using **standard competition ranking**; no secondary tiebreaker is applied.
- **FR-028**: The system MUST display updated standings and match results to all users within 5 seconds of a result being recorded or corrected, without requiring a full page reload. Updates MUST be pushed to connected clients via **SignalR**; the Angular frontend MUST subscribe to the relevant hub and update standings and the match list in place. Frontend UX requirements:
  - **In-place refresh**: Standings and match list refresh automatically on receiving an update.
  - **Visual update indicator**: A brief visual indication (row highlight or transient notification) signals that data has changed.
  - **Connection loss warning**: A warning is shown if the real-time connection is interrupted.
  - **Automatic reconnection**: The SignalR client reconnects automatically after a connection loss without user action.

### Key Entities

- **Standing**: Computed statistics for a team within a tournament. Includes: position (rank), total points, matches played, matches won, matches drawn, and matches lost. Derived from all recorded match results and the tournament's scoring system.
- **Match**: A recorded contest between a home team and an away team. Captures score (goals/points per team) and the date/time the match took place.

---

## Success Criteria *(mandatory)*

- **SC-004**: Match result updates become visible to all users within 5 seconds of the Organizer recording them, without a manual page refresh.
- **SC-008**: The standings table always reflects the complete and correct accumulation of all recorded match results using the tournament's scoring system — no result is omitted and no correction is ignored.

---

## Assumptions

- **Real-time updates**: Near-real-time result visibility is required. Updates are delivered via **SignalR** (ASP.NET Core WebSocket hub); the Angular frontend subscribes to tournament-scoped hub events.
- **Tournament visibility**: Tournaments in "Open", "In Progress", or "Completed" status are publicly accessible without authentication. "Draft" tournaments are only visible to their owning Organizer.
