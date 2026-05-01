# Tasks Checklist: Esports Tournament Management Platform

**Purpose**: Validates whether implementation tasks are complete, clearly specified, correctly ordered, and cover all requirements — treating tasks.md as a requirements document for "what must be built, in what order, and by whom."
**Created**: 2026-04-20
**Feature**: [tasks.md](../tasks.md)
**Audience**: Author (self-review before implementation starts)
**Focus**: FR coverage, dependency ordering, parallelism consistency, test task completeness, file path specificity, missing tasks

---

## Requirement Completeness

- [x] CHK001 — Is there a task for configuring CORS middleware and HTTPS redirection as part of Phase 2 foundational setup, given both are security baseline requirements with no current task allocated? [Completeness, Gap]
  > **Cross-check:** RESOLVED — T087 added: configure CORS policy and HTTPS redirection in backend/EsportsApp.API/Program.cs.
- [x] CHK002 — Is there a task for seeding the bootstrap Admin account at startup (Spec §Assumptions: "at least one Admin account exists at system startup"), separate from the videogame catalog seed task T017? [Completeness, Spec §Assumptions, Gap]
  > **Cross-check:** PASS — T017 explicitly lists "admin bootstrap data" alongside videogame catalog seed in the same SeedDataInitializer.cs file. Combined seeding in one task is appropriate.
- [x] CHK003 — Is there a task explicitly covering the `lang` attribute on Angular's `index.html` (Constitution §Accessibility baseline), or is it silently assumed to be part of T002 Angular workspace setup? [Completeness, Spec §plan.md §Constitution, Gap]
  > **Cross-check:** RESOLVED — T002 updated to include setting `lang` attribute on frontend/src/index.html.
- [x] CHK004 — Are T004 (.env.example) and T007 (.gitignore) scoped specifically enough to guarantee all required secret variable names are documented and no secrets are committed — or are they described too broadly? [Completeness, Spec tasks.md §T004, §T007]
  > **Cross-check:** PASS — T004 requires documenting "backend environment template variables"; T007 requires "gitignore rules for env and build artifacts." Sufficient for academic scope.
- [x] CHK005 — Is there a task for the Player account update endpoint (PATCH /api/users/me — FR-038) as a distinct backend implementation task, separate from the profile page UI task T033? [Completeness, Spec §FR-038, Gap]
  > **Cross-check:** PASS — T028: "Implement user profile service for GET/PATCH me rules" and T030: "Implement user profile endpoints from contracts/users.md" cover the backend; T033 covers the Angular UI.

## Requirement Clarity

- [x] CHK006 — Is the scope of T016 (global exception middleware) sufficient to cover FR-040, FR-041, FR-042, and FR-043 — do the task description or FR references make all three error response classes (400/409-422/500) explicit? [Clarity, Spec tasks.md §T016, Spec §FR-040–§FR-043]
  > **Cross-check:** PASS — T016 description references FR-040 and "RFC-style error payloads"; T086 adds unit tests covering HTTP 400/409/422/500 error mapping, ensuring all four FRs are explicitly tested.
- [x] CHK007 — Is the scope of T019 (SignalR hub contract) sufficient to cover hub authentication requirements, group join/leave endpoints, and the exact server-to-client method signature `ReceiveStandingsUpdate` with its payload schema? [Clarity, Spec tasks.md §T019, Spec §FR-028]
  > **Cross-check:** PASS — T019 implements TournamentHub.cs; the full hub contract (auth, group join, ReceiveStandingsUpdate, payload) is in contracts/standings.md for implementers to follow.
- [x] CHK008 — Is T085 (FR-to-unit-test traceability matrix) scoped clearly enough — does it specify which FRs are in scope (FR-001 through FR-039 excluding reserved entries), the expected output format, and the file location? [Clarity, Spec tasks.md §T085, Spec §SC-007]
  > **Cross-check:** RESOLVED — T085 expanded to specify FR-001 through FR-043 scope, table format (FR id, description, linked test class/method), coverage notes for manual-only FRs, and the output file path.
- [x] CHK009 — Are T082 and T083 (Suspended badge for public tournament visibility) explicitly scoped to both the tournament listing page and the tournament detail page, or only one? [Clarity, Spec tasks.md §T082, §T083, Spec §FR-012]
  > **Cross-check:** PASS — T083: "Display Suspended status badge on Angular public tournament list and detail pages (FR-012)" — both pages are named.

## Requirement Consistency

- [x] CHK010 — Are all tasks tagged [P] (parallelizable) consistent with the stated parallelism opportunities in the "Parallel Opportunities" section — are there [P]-tagged tasks that have hidden sequential dependencies on incomplete sibling tasks? [Consistency, Spec tasks.md §Parallel Opportunities]
  > **Cross-check:** RESOLVED — T013 [P] tag removed; T013 (implement repositories) depends on T012 (define interfaces) and must run sequentially.
- [x] CHK011 — Do all Phase 2 foundational tasks (T008–T021) have explicit "depends on Phase 1" documentation, or are some floating without documented prerequisites? [Consistency, Spec tasks.md §Phase 2]
  > **Cross-check:** PASS — tasks.md Phase 2 header: "⚠️ CRITICAL: No user story implementation starts until this phase is complete." Phase-level dependency is documented in the Dependencies section.
- [x] CHK012 — Are the test tasks for all TR-required stories consistently structured — do all four required stories (TR-001 to TR-004) have both unit test tasks and manual test doc tasks? [Consistency, Spec §TR-001–§TR-004, Spec tasks.md §Phases 3–8]
  > **Cross-check:** PASS — TR-001 (T022–T025), TR-002 (T034–T036), TR-003 (T052–T053), TR-004 (T066–T068) all have both unit tests and manual test doc tasks.

## Acceptance Criteria Quality

- [x] CHK013 — Do each phase's checkpoint descriptions specify measurable entry and exit conditions, or are checkpoints described in general terms that cannot be objectively verified before moving to the next phase? [Measurability, Spec tasks.md §Checkpoints]
  > **Cross-check:** PASS — Each checkpoint states the independently testable condition per user story (e.g., "User Story 1 is fully functional and independently testable"); independent test procedures are documented per story in spec.md.
- [x] CHK014 — Is T079 (run backend test suite and capture results) specific about expected pass rate, output format, and exact file location of the results summary — or is it too vague to verify completion? [Measurability, Spec tasks.md §T079]
  > **Cross-check:** PASS — T079 targets backend/EsportsApp.Tests/TestResults/summary.md; expected pass rate is 100% for unit tests (any failure blocks the milestone). The summary file is the completion artifact.
- [x] CHK015 — Is T080 (end-to-end manual verification) linked to a concrete checklist or script, or does it rely on an informal walkthrough without defined acceptance criteria? [Measurability, Spec tasks.md §T080]
  > **Cross-check:** RESOLVED — T080 updated to record outcomes in specs/001-esports-tournament-platform/checklists/e2e-results.md (not in requirements.md, which is the spec quality checklist).

## Scenario Coverage

- [x] CHK016 — Is there a task for the FR-033 scoring strategy extensibility pattern — specifically, defining the strategy interface and all three concrete implementations (Standard, WinnerTakesAll, Custom) before T070 service logic builds on them? [Coverage, Spec §FR-033, Spec tasks.md §T069]
  > **Cross-check:** PASS — T069: "Implement scoring strategy interfaces and concrete strategies in backend/EsportsApp.Application/Scoring/" precedes T070 in Phase 8.
- [x] CHK017 — Are there corresponding frontend and unit test tasks for the tournament deletion flow (FR-011) alongside backend task T081, or is the deletion feature only partially covered? [Coverage, Spec tasks.md §T081, Spec §FR-011, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — T081 covers the backend. Deletion UI is part of T042 (organizer tournament create/edit/delete UI). A separate unit test for deletion is accepted as covered by T035 tournament lifecycle tests.
- [x] CHK018 — Is T084 (meta description and SEO baseline) scoped to both the public tournament listing page and the tournament detail page, or does it cover only one? [Coverage, Spec tasks.md §T084, Spec §plan.md §Constitution]
  > **Cross-check:** PASS — T084: "Add meta description and single descriptive h1 to public Angular tournament list and detail pages" — both pages are named.

## Dependencies & Ordering

- [x] CHK019 — Is the US5/US6 ordering constraint (US6 must be complete before US5 standings and SignalR components are finalized) documented within individual US5 tasks, not only in the dependency graph section? [Clarity, Spec tasks.md §Dependencies, §Phase 7]
  > **Cross-check:** PASS — tasks.md Phase 7 header and the Dependencies §User Story Dependency Graph both state that live standings and SignalR (FR-028, FR-039) require Phase 8 (US6) to be complete before Phase 7 is finalized.
- [x] CHK020 — Is the Admin management feature (T074, T075 in Phase 9) explicitly documented as depending on T014 (JWT role policies from Phase 2), ensuring an implementer cannot start Admin endpoints before auth infrastructure is in place? [Clarity, Spec tasks.md §Phase 9, §T014]
  > **Cross-check:** PASS — tasks.md Phase 9: "Depends on all selected user stories." Phase 2 (which contains T014) blocks all user stories, which in turn block Phase 9. The transitive dependency is documented.

---
