# API / Contracts Checklist: Esports Tournament Management Platform

**Purpose**: Validates whether API contract requirements are complete, clear, and consistent — covering endpoint definitions, error handling, authentication requirements, response schemas, and the SignalR hub contract.
**Created**: 2026-04-20
**Feature**: [contracts/](../contracts/)
**Audience**: Author (self-review before implementation starts)
**Focus**: Endpoint completeness vs FRs, error response consistency, auth coverage, pagination, SignalR contract

---

## Requirement Completeness

- [x] CHK001 — Are all FRs from spec.md mapped to at least one documented API endpoint — specifically, are FR-008 (Admin user management), FR-009 (Player profile read), FR-037 (Organizer account update), and FR-038 (Player account update) each covered by a contract file? [Completeness, Gap, Spec §data-model.md §Videogame, Gap]
  > **Cross-check:** PASS — contracts/users.md covers FR-009 (GET /api/users/me), FR-037 (PATCH /api/users/me — Organizer), FR-038 (PATCH /api/users/me — Player), and FR-008 (GET /api/users, PATCH role, POST suspend/reinstate — Admin).
- [x] CHK002 — Is there a documented endpoint for GET /api/videogames to serve the fixed videogame catalog used in Player registration and team creation dropdowns? [Completeness, Spec §data-model.md §Videogame, Gap]
  > **Cross-check:** PASS — contracts/videogames.md documents GET /api/videogames with response schema and notes.
- [x] CHK003 — Are tournament registration endpoints (POST to register a team, DELETE or PATCH to withdraw) documented with full request/response schemas in a contract file? [Completeness, Spec §FR-022, §FR-024, Gap]
  > **Cross-check:** PASS — POST /api/tournaments/{tournamentId}/registrations and DELETE /api/tournaments/{tournamentId}/registrations/{teamId} are documented at the bottom of contracts/teams.md with full schemas.
- [x] CHK004 — Is the SignalR hub contract fully documented — including hub URL, group join/leave protocol, whether authentication is required to connect, and the exact server-to-client method name and payload schema (`ReceiveStandingsUpdate`)? [Completeness, Spec §FR-028, Gap]
  > **Cross-check:** PASS — contracts/standings.md SignalR section documents hub URL (/hubs/tournament), auth requirement (Bearer JWT required), group join protocol (JoinTournamentGroup), ReceiveStandingsUpdate method name, and full payload schema.
- [x] CHK005 — Are all team management endpoints (create team, invite player, accept/decline invitation, remove member, transfer captaincy) documented with request/response schemas in contracts/teams.md? [Completeness, Spec §FR-016–§FR-021, Gap]
  > **Cross-check:** PASS — contracts/teams.md documents all five operations with full request/response schemas.

## Requirement Clarity

- [x] CHK006 — Is the Suspended tournament behavior in GET /api/tournaments response explicitly documented — that Suspended tournaments appear in public listings only if previously Open/InProgress/Completed, with status set to "Suspended"? [Clarity, Spec §FR-012, Gap]
  > **Cross-check:** RESOLVED — contracts/tournaments.md GET /api/tournaments behavior updated: unauthenticated and non-owning authenticated users see Open, InProgress, Completed, and Suspended; owning Organizer additionally sees their Draft.
- [x] CHK007 — Is the scoring system immutability rule (cannot change once InProgress) enforced at the contract level with a specific HTTP status code and error body documented for a rejected change attempt? [Clarity, Spec §FR-032]
  > **Cross-check:** PASS — contracts/tournaments.md PUT /api/tournaments/{id} returns 422 Unprocessable Entity for "Attempted to change scoring system after InProgress".
- [x] CHK008 — Is the distinction between 409 Conflict (uniqueness violations) and 422 Unprocessable Entity (domain rule violations) documented consistently and explicitly across all contract files? [Clarity, Spec §FR-042]
  > **Cross-check:** PASS — 409 is used for uniqueness violations (duplicate username, duplicate match), 422 for domain rule violations (scoring locked, wrong game) consistently across all contracts.
- [x] CHK009 — Are immutable fields (Player username, Player main videogame, Organizer email) explicitly identified in the update schemas as non-updatable, with the expected error response code and body documented? [Clarity, Spec §FR-037, §FR-038]
  > **Cross-check:** PASS — contracts/users.md PATCH /api/users/me Notes: "Players cannot change username or mainVideogame (FR-007). Organizers cannot change email (FR-037)."
- [x] CHK010 — Is the Draft-tournament visibility restriction documented at the GET /api/tournaments/{id} contract level, with the expected HTTP response code (404 or 403) for a non-owning requester attempting to access a Draft tournament? [Clarity, Spec §FR-012, §FR-015]
  > **Cross-check:** PASS — contracts/tournaments.md GET /{id}: 404 returned for "Not found or Draft not owned by caller".

## Requirement Consistency

- [x] CHK011 — Are pagination parameters (page, pageSize, totalCount, items) consistently defined across all list endpoints that return collections? [Consistency, Spec §contracts/tournaments.md]
  > **Cross-check:** PASS — Paginated responses (tournaments list, admin user list) share the same `{items, totalCount, page, pageSize}` shape. Bounded per-tournament collections (standings, match history, team roster) return flat arrays — appropriate for academic scale.
- [x] CHK012 — Are error response body schemas consistent across all contracts — does every 400 use `{errors: {field: [messages]}}` and every 409/422/403 use `{message: string}`, as implied by FR-041/FR-042? [Consistency, Spec §FR-041, §FR-042]
  > **Cross-check:** PASS — 400 uses `{errors: {field: [messages]}}` and 409/422 use `{message: "..."}` consistently across all contract files.
- [x] CHK013 — Are Bearer JWT auth requirements documented consistently for all protected endpoints — is there any write endpoint in any contract file missing an explicit auth requirement? [Consistency, Spec §FR-004, §FR-005]
  > **Cross-check:** PASS — All write endpoints document auth role requirement and 401/403 responses.
- [x] CHK014 — Are all identifier fields (team IDs, player IDs, tournament IDs) consistently represented as GUIDs (not integers) across all contract request and response schemas? [Consistency, Spec §data-model.md]
  > **Cross-check:** PASS — All ID fields are typed as `guid` throughout all contract files.

## Acceptance Criteria Quality

- [x] CHK015 — Does each documented API endpoint enumerate all expected HTTP response status codes (including 401, 403, 404, 409, 422 where applicable), or are any error paths left undocumented? [Completeness, Spec §FR-041–§FR-042]
  > **Cross-check:** PASS — All endpoints list applicable status codes. Self-match 422 added to POST /api/tournaments/{id}/matches.
- [x] CHK016 — Is the full standings recalculation guarantee documented at the contract level for PUT /api/tournaments/{id}/matches/{matchId} — specifically, that the response reflects the fully recalculated standings state? [Completeness, Spec §FR-027]
  > **Cross-check:** PASS — contracts/matches.md PUT Notes: "After persisting, a full standings recalculation is triggered and a SignalR push is sent."

## Scenario Coverage

- [x] CHK017 — Is the tournament listing behavior documented for all three caller types (unauthenticated visitor, authenticated non-owning user, owning Organizer) and their respective visibility scopes — including Draft and Suspended? [Coverage, Spec §FR-012, §FR-015]
  > **Cross-check:** RESOLVED — contracts/tournaments.md GET /api/tournaments now documents all three caller types: unauthenticated (Open/InProgress/Completed/Suspended), authenticated non-owner (same), owning Organizer (additionally their Draft).
- [x] CHK018 — Are empty-collection responses documented for list endpoints — e.g., is GET /api/tournaments/{id}/matches when no matches exist specified as 200 with `[]` rather than 404? [Coverage, Gap]
  > **Cross-check:** PASS — contracts/matches.md GET explicitly states "200 OK | List returned (empty array if no matches yet)". All other collection endpoints follow the same 200+empty-array convention.
- [x] CHK019 — Is the authentication requirement of the SignalR hub documented — specifically, whether anonymous connections are accepted (for public match list updates) or if auth is required (given standings are auth-gated per FR-039)? [Coverage, Spec §FR-028, §FR-039, Gap]
  > **Cross-check:** RESOLVED — contracts/standings.md SignalR section now states: Bearer JWT required to establish connection; anonymous connections are rejected.
- [x] CHK020 — Are Admin-only endpoints (suspend/reinstate Organizer, change user roles) explicitly scoped to the Admin role in the contract documentation, with 403 documented for lower-privilege callers? [Coverage, Spec §FR-008]
  > **Cross-check:** PASS — contracts/users.md marks all Admin-only endpoints with "*(Admin only)*" and documents 403 for non-Admin callers.

---
