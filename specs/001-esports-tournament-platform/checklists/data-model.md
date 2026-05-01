# Data Model Checklist: Esports Tournament Management Platform

**Purpose**: Validates whether data model requirements are complete, clear, and consistent — covering entity definitions, constraints, relationships, and behavioral rules documented in data-model.md.
**Created**: 2026-04-20
**Feature**: [data-model.md](../data-model.md)
**Audience**: Author (self-review before implementation starts)
**Focus**: Entity completeness, constraint clarity, relationship coverage, cascade rules, enum coverage, behavioral rules

---

## Requirement Completeness

- [x] CHK001 — Are cascade delete behaviors defined for all FK relationships — specifically, what happens to a Team's Match rows, Standing rows, and TournamentRegistration rows if a Team is removed from the system? [Completeness, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Teams are never deleted from the system; they can only lose registration via Withdrawn status. Cascade specifics (e.g., preventing orphaned Match rows) are delegated to EF Core configuration at implementation time.
- [x] CHK002 — Is the Player–User (1:1 shared PK) relationship explicitly described so implementers know that Player.Id equals User.Id and no surrogate FK is needed? [Completeness, Spec §data-model.md §Player]
  > **Cross-check:** PASS — data-model.md: `Player.Id | Guid | PK, FK → User.Id (1:1)`; same for Organizer. EF Core Configuration Notes mention TPH/TPT options.
- [x] CHK003 — Are all default values explicitly enumerated for fields with defaults — MinMembersPerTeam (5), TournamentStatus (Draft), InvitationStatus (Pending), RegistrationStatus (Active), User.IsActive (true) — or are any defaults left implicit? [Completeness, Spec §data-model.md §Tournament, §TeamInvitation, §TournamentRegistration]
  > **Cross-check:** PASS — All five defaults are explicitly stated in data-model.md entity tables.
- [x] CHK004 — Are database index requirements documented for performance-critical query paths — e.g., standings ordered by Points DESC, match lookup by TournamentId, invitation lookup by InvitedPlayerId? [Completeness, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Index strategy delegated to EF Core implementation. UNIQUE constraints double as indexes for the most critical query paths (TournamentId+TeamId on Standing).
- [x] CHK005 — Is the Standing entity row lifecycle documented — specifically, when is a Standing row created (on team registration, or only after the first match is recorded), and is it ever deleted or only updated? [Completeness, Gap]
  > **Cross-check:** RESOLVED — data-model.md Standing Lifecycle added: row created on Active registration (zeros), updated on match record/correction, deleted on withdrawal.

## Requirement Clarity

- [x] CHK006 — Is the PreSuspensionStatus nullable pattern fully documented with its valid state machine — when is it set (on suspension), when is it cleared (on reinstatement), and can it hold Suspended as its own value? [Clarity, Spec §data-model.md §Tournament]
  > **Cross-check:** PASS — data-model.md Tournament entity defines PreSuspensionStatus nullable; state diagram shows it is set on Admin suspension (saves current status) and cleared on reinstatement. It cannot hold Suspended as its own value by design.
- [x] CHK007 — Is the "lazy expiry evaluation" rule for TeamInvitation explicitly documented as the authoritative behavior (no background job; expiry checked only on read) so implementers do not add an unnecessary scheduled task? [Clarity, Spec §data-model.md §TeamInvitation]
  > **Cross-check:** PASS — data-model.md TeamInvitation Expiry rule: "lazy — no background job required."
- [x] CHK008 — Is the "Position computed at query time" rule for the Standing position clearly distinguished from a stored column, ensuring implementers do not add a persisted Position field that could become stale? [Clarity, Spec §data-model.md §Standing]
  > **Cross-check:** PASS — data-model.md Standing Notes: "Position (rank) is computed at query time by ORDER BY Points DESC; not stored."
- [x] CHK009 — Is the ScoringSystem type-to-field mapping documented — specifically, who sets WinPoints/DrawPoints/LossPoints for Standard and WinnerTakesAll types (system-fixed), versus Custom (Organizer-supplied)? [Clarity, Spec §data-model.md §ScoringSystem, §FR-029]
  > **Cross-check:** PASS — data-model.md ScoringSystem Validation: Standard and WinnerTakesAll values are "set by system"; Custom is Organizer-supplied with the inequality constraint.
- [x] CHK010 — Is the `HomeTeamId ≠ AwayTeamId` rule documented at both DB level (CHECK constraint) and service layer, or only at one level — and is the expected error response for a self-match attempt specified? [Clarity, Spec §data-model.md §Match]
  > **Cross-check:** RESOLVED — data-model.md has a DB CHECK constraint. contracts/matches.md POST 422 now includes `homeTeamId equals awayTeamId` as a rejection condition.

## Requirement Consistency

- [x] CHK011 — Are UTC timestamp semantics consistently documented for all DateTime fields (CreatedAt, RegisteredAt, RecordedAt, JoinedAt) — are there any datetime fields that intentionally omit UTC specification? [Consistency, Spec §data-model.md]
  > **Cross-check:** RESOLVED — Match.PlayedAt updated to NOT NULL, UTC in data-model.md. All DateTime fields now carry the UTC qualifier.
- [x] CHK012 — Is the uniqueness scope for Team.Name (per VideogameId) consistently stated in both data-model.md and spec.md FR-016, with no location implying global team name uniqueness? [Consistency, Spec §data-model.md §Team vs §FR-016]
  > **Cross-check:** PASS — data-model.md Team: UNIQUE(Name, VideogameId). spec.md FR-016: "unique within that videogame." Consistent.
- [x] CHK013 — Is the ScoringSystem uniqueness (UNIQUE TournamentId — 1:1 with Tournament) consistently expressed in data-model.md and the scoring system FRs — are there no places that imply a tournament can have multiple scoring systems? [Consistency, Spec §data-model.md §ScoringSystem vs §FR-029]
  > **Cross-check:** PASS — data-model.md ScoringSystem: TournamentId is UNIQUE (1:1). No FR implies multiple scoring systems per tournament.

## Scenario Coverage

- [x] CHK014 — Is the team disbandment scenario defined at the data model level — what happens to data when the last Captain leaves a team with no other members? [Coverage, Spec §Edge Cases, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — spec.md Edge Cases documents the behavioral rule (team disbanded). Data deletion of the Team row and associated TeamMember/TeamInvitation rows is delegated to service-layer implementation.
- [x] CHK015 — Is the team-withdrawal-from-tournament scenario documented at the data model level — does TournamentRegistration.Status change to Withdrawn (soft delete) or is the row removed entirely? [Coverage, Spec §FR-024, §data-model.md §TournamentRegistration]
  > **Cross-check:** PASS — RegistrationStatus enum has Active and Withdrawn values; TournamentRegistration.Status defaults to Active and is set to Withdrawn on withdrawal (soft delete).
- [x] CHK016 — Is the Organizer suspension scenario fully covered — does PreSuspensionStatus reliably capture the pre-suspension tournament state for all tournaments, and is the reinstatement query deterministic? [Coverage, Spec §FR-008, §FR-012]
  > **Cross-check:** PASS — data-model.md Tournament has nullable PreSuspensionStatus. State diagram shows set-on-suspend / clear-on-reinstate. Query is deterministic: restore PreSuspensionStatus to Status, set PreSuspensionStatus to null.

## Non-Functional & Constraint Quality

- [x] CHK017 — Are ScoringSystem validation constraints (WinPoints ≥ DrawPoints ≥ LossPoints ≥ 0) classified as service-layer only, or also enforced at the DB level via CHECK constraints? [Completeness, Spec §data-model.md §ScoringSystem]
  > **Cross-check:** PASS — data-model.md explicitly states: "Validation (service layer, enforced at creation)". No DB CHECK constraint is added for scoring inequality, which is appropriate since point values differ per type.
- [x] CHK018 — Are all service-layer constraints explicitly separated from DB-level constraints throughout data-model.md so implementers know at which layer to enforce each rule? [Clarity, Spec §data-model.md]
  > **Cross-check:** PASS — data-model.md consistently uses "DB Constraints" and "Service-Layer Constraints" subsections per entity, and the Key Validation Rules Summary table labels each rule's enforcement point.
- [x] CHK019 — Is the Match duplicate detection rule (same TournamentId, same two teams regardless of home/away order, same PlayedAt) sufficient to prevent all semantic duplicates — or could the same real match be stored twice with home/away swapped? [Coverage, Spec §FR-035, §data-model.md §Match]
  > **Cross-check:** PASS — data-model.md: "reject if a match already exists for the same TournamentId and the same two teams (regardless of home/away order) at the same PlayedAt." Home/away swap is explicitly prevented.
- [x] CHK020 — Are all enum types documented with their complete set of valid values and semantic meaning, or are any enum definitions left implicit or undocumented? [Completeness, Spec §data-model.md §Enums]
  > **Cross-check:** PASS — data-model.md Enums section documents all five enums (UserRole, TournamentStatus, ScoringSystemType, InvitationStatus, RegistrationStatus) with complete value lists.

---
