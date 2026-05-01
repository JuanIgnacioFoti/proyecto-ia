# Tasks: Esports Tournament Management Platform

**Input**: Design documents from `/specs/001-esports-tournament-platform/`  
**Plan**: [plan.md](plan.md) (last updated 2026-04-24)  
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/  
**Feature Specs**: [features/](features/) — one self-contained spec per deliverable unit  

**Tests**: Unit tests and manual test cases are required by spec (TR-001 to TR-004, SC-007).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing. Each phase links to the corresponding per-feature spec file.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (`[US1]`, `[US2]`, etc.)
- Include exact file paths in every task description

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize solution, projects, and baseline tooling.

- [X] T001 Create .NET solution and backend projects in backend/EsportsApp.sln
- [X] T002 Create Angular 21 workspace and app shell in frontend/angular.json; set `lang` attribute on frontend/src/index.html (Constitution §Accessibility)
- [X] T003 [P] Add Docker compose skeleton for api, frontend, and sqlserver in docker-compose.yml
- [X] T004 [P] Add backend environment template variables in backend/EsportsApp.API/.env.example
- [X] T005 [P] Configure Swagger and API base services in backend/EsportsApp.API/Program.cs
- [X] T006 [P] Configure frontend environment endpoints in frontend/src/environments/environment.ts
- [X] T007 [P] Add root and backend gitignore rules for env and build artifacts in .gitignore

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core architecture and infrastructure that blocks all user stories.

**⚠️ CRITICAL**: No user story implementation starts until this phase is complete.

- [X] T008 Define core enums for roles, statuses, and scoring in backend/EsportsApp.Domain/Enums/
- [X] T009 Define base domain entities and relationships in backend/EsportsApp.Domain/Entities/
- [X] T010 Implement EF Core DbContext and entity configurations including `UNIQUE(TournamentId, TeamId)` on TournamentRegistration (FR-023) in backend/EsportsApp.Infrastructure/Data/AppDbContext.cs
- [X] T011 Create initial EF Core migration for core schema in backend/EsportsApp.Infrastructure/Data/Migrations/
- [X] T012 [P] Implement repository interfaces for aggregate access in backend/EsportsApp.Infrastructure/Interfaces/
- [X] T013 Implement repository classes using EF Core in backend/EsportsApp.Infrastructure/Repositories/
- [X] T014 Configure JWT authentication and role policies; set `ClockSkew = TimeSpan.Zero` in `TokenValidationParameters` to prevent expired tokens being accepted within the default 5-minute grace window (FR-002) in backend/EsportsApp.API/Program.cs
- [X] T015 Implement token generation service and auth options binding in backend/EsportsApp.Application/Services/JwtTokenService.cs
- [X] T016 Implement global exception middleware and RFC-style error payloads in backend/EsportsApp.API/Middleware/GlobalExceptionMiddleware.cs
- [X] T017 [P] Seed fixed videogame catalog and admin bootstrap data in backend/EsportsApp.Infrastructure/Data/SeedDataInitializer.cs
- [X] T018 [P] Create shared request/response DTO contracts and validation attributes in backend/EsportsApp.Application/DTOs/
- [X] T019 [P] Add SignalR tournament hub contract and group join endpoint in backend/EsportsApp.API/Hubs/TournamentHub.cs
- [X] T020 Register dependency injection wiring for all repositories/services in backend/EsportsApp.API/Program.cs
- [X] T021 Add frontend auth interceptor, route guards, and API error handler in frontend/src/app/core/
- [X] T087 [P] Configure CORS policy (allowed origins, credentials) and HTTPS redirection middleware in backend/EsportsApp.API/Program.cs

**Checkpoint**: Foundation ready; user stories can now be implemented.

---

## Phase 3: User Story 1 - Registration and Login (Priority: P1) 🎯 MVP

> Spec: [features/001-registration-and-login/spec.md](features/001-registration-and-login/spec.md)

**Goal**: Enable Player/Organizer registration, login with JWT, and own-profile view/update. Admin user management (list, suspend/reinstate).

**Independent Test**: Register one Player and one Organizer, log in, access protected endpoint with role checks (401/403), and update allowed profile fields successfully.

### Tests for User Story 1

- [ ] T022 [P] [US1] Add Player registration unit tests (TR-001) in backend/EsportsApp.Tests/Unit/Application/Services/PlayerServiceRegistrationTests.cs
- [ ] T023 [P] [US1] Add Organizer registration validation unit tests in backend/EsportsApp.Tests/Unit/Application/Services/OrganizerServiceRegistrationTests.cs
- [ ] T024 [P] [US1] Add JWT login and token expiry tests; verify HTTP 401 is returned for a token expired at exact boundary with no skew tolerance (`ClockSkew = TimeSpan.Zero`) (FR-002) in backend/EsportsApp.Tests/Unit/Application/Services/AuthServiceLoginTests.cs
- [ ] T025 [P] [US1] Document manual test cases for TR-001 auth/profile flows in backend/EsportsApp.Tests/Manual/TR-001-player-registration.md
- [ ] T093 [P] [US1] Document manual test cases for TR-001 Organizer registration flow (FR-036: org name uniqueness/length, password complexity, duplicate rejection) in backend/EsportsApp.Tests/Manual/TR-001-organizer-registration.md

### Implementation for User Story 1

- [X] T026 [P] [US1] Implement auth DTOs and validators for register/login in backend/EsportsApp.Application/DTOs/Auth/
- [X] T027 [P] [US1] Implement user/account service logic for registration and login in backend/EsportsApp.Application/Services/AuthService.cs
- [X] T028 [US1] Implement user profile service for Player GET/PATCH me (FR-009, FR-038) and Organizer PATCH me (FR-037: org name uniqueness, length, password complexity) in backend/EsportsApp.Application/Services/UserService.cs
- [X] T029 [US1] Implement auth endpoints from contracts/auth.md in backend/EsportsApp.API/Controllers/AuthController.cs
- [X] T030 [US1] Implement user profile endpoints from contracts/users.md in backend/EsportsApp.API/Controllers/UsersController.cs
- [X] T031 [P] [US1] Build Angular login and registration pages in frontend/src/app/features/auth/
- [X] T032 [US1] Connect Angular auth service and token storage flow in frontend/src/app/core/services/auth.service.ts
- [X] T033 [US1] Add profile page edit form for Player/Organizer allowed fields in frontend/src/app/features/auth/profile-page.component.ts

**Checkpoint**: User Story 1 is fully functional and independently testable.

---

## Phase 4: User Story 2 - Tournament Creation and Management (Priority: P2)

> Spec: [features/002-tournament-management/spec.md](features/002-tournament-management/spec.md)

**Goal**: Organizer creates, edits, views, and advances tournament lifecycle with scoring constraints. Suspended state handled by Admin reinstatement.

**Independent Test**: Organizer creates Draft tournament, verifies private visibility, advances to Open for public visibility, and unauthorized modifications return 403.

### Tests for User Story 2

- [ ] T034 [P] [US2] Add tournament creation validation unit tests (TR-002) in backend/EsportsApp.Tests/Unit/Application/Services/TournamentServiceCreationTests.cs
- [ ] T035 [P] [US2] Add tournament lifecycle transition and delete-ownership unit tests in backend/EsportsApp.Tests/Unit/Application/Services/TournamentServiceLifecycleTests.cs
- [ ] T036 [P] [US2] Document manual test cases for TR-002 creation, lifecycle, and delete flow in backend/EsportsApp.Tests/Manual/TR-002-tournament-creation.md

### Implementation for User Story 2

- [X] T037 [P] [US2] Implement tournament DTOs and scoring config validators in backend/EsportsApp.Application/DTOs/Tournaments/
- [X] T038 [US2] Implement tournament application service with ownership and transition rules in backend/EsportsApp.Application/Services/TournamentService.cs
- [X] T039 [US2] Implement tournaments list/detail/create/update/advance endpoints in backend/EsportsApp.API/Controllers/TournamentsController.cs
- [X] T081 [US2] Implement DELETE /api/tournaments/{id} endpoint with ownership guard (FR-011) in backend/EsportsApp.API/Controllers/TournamentsController.cs
- [X] T040 [P] [US2] Implement organizer tournament repository queries for visibility filters in backend/EsportsApp.Infrastructure/Repositories/TournamentRepository.cs
- [X] T082 [US2] Update tournament listing query to include Suspended tournaments in public results with Suspended status (FR-012) in backend/EsportsApp.Infrastructure/Repositories/TournamentRepository.cs
- [X] T083 [US2] Display Suspended status badge on Angular public tournament list and detail pages (FR-012) in frontend/src/app/features/tournaments/
- [X] T041 [P] [US2] Build Angular tournament list and detail pages with Draft visibility behavior in frontend/src/app/features/tournaments/
- [X] T042 [US2] Build Angular organizer tournament create/edit, status advance, and delete UI in frontend/src/app/features/tournaments/organizer-tournament-form.component.ts

**Checkpoint**: User Stories 1 and 2 work independently.

---

## Phase 5: User Story 3 - Team Formation and Player Management (Priority: P3)

> Spec: [features/003-team-formation/spec.md](features/003-team-formation/spec.md)

**Goal**: Players create teams, manage invitations, accept/decline invites, remove members, transfer captaincy, and disband teams (with automatic registration withdrawal).

**Independent Test**: Player creates a team, invites another player by username/email, invited player accepts, and duplicate/same-game membership constraints are enforced.

### Tests for User Story 3

- [ ] T043 [P] [US3] Add team creation and captain constraints unit tests in backend/EsportsApp.Tests/Unit/Application/Services/TeamServiceCreationTests.cs
- [ ] T044 [P] [US3] Add invitation accept/decline and expiry unit tests in backend/EsportsApp.Tests/Unit/Application/Services/TeamServiceInvitationTests.cs
- [ ] T045 [P] [US3] Add membership and captaincy transfer rule tests in backend/EsportsApp.Tests/Unit/Application/Services/TeamServiceMembershipTests.cs

### Implementation for User Story 3

- [X] T046 [P] [US3] Implement team and invitation DTOs in backend/EsportsApp.Application/DTOs/Teams/
- [X] T047 [US3] Implement team service with captain, membership, invitation workflows, and team-disband logic in backend/EsportsApp.Application/Services/TeamService.cs
- [X] T048 [US3] Implement teams and invitation endpoints from contracts/teams.md in backend/EsportsApp.API/Controllers/TeamsController.cs
- [X] T049 [P] [US3] Implement invitation persistence and lookup queries in backend/EsportsApp.Infrastructure/Repositories/TeamInvitationRepository.cs
- [X] T090 [US3] Implement auto-withdrawal of active tournament registrations when a team is disbanded (edge case from features/003-team-formation/spec.md + features/004-tournament-registration/spec.md) in backend/EsportsApp.Application/Services/TeamService.cs
- [X] T050 [P] [US3] Build Angular team management and roster UI in frontend/src/app/features/teams/
- [X] T051 [US3] Build Angular player invitation inbox and accept/decline actions in frontend/src/app/features/teams/player-invitations.component.ts

**Checkpoint**: User Stories 1 to 3 are independently functional.

---

## Phase 6: User Story 4 - Tournament Registration (Priority: P4)

> Spec: [features/004-tournament-registration/spec.md](features/004-tournament-registration/spec.md)

**Goal**: Team captains register/withdraw teams in Open tournaments with full eligibility validation. Disbanded teams auto-withdraw their registration.

**Independent Test**: Captain registers an eligible team in an Open tournament, appears in participant list, duplicate/different-game/full-capacity/insufficient-members validations fail correctly.

### Tests for User Story 4

- [ ] T052 [P] [US4] Add tournament registration eligibility unit tests (TR-003) in backend/EsportsApp.Tests/Unit/Application/Services/TournamentRegistrationServiceTests.cs
- [ ] T053 [P] [US4] Document manual test cases for TR-003 registration and withdrawal in backend/EsportsApp.Tests/Manual/TR-003-tournament-registration.md

### Implementation for User Story 4

- [X] T054 [P] [US4] Implement tournament registration DTOs in backend/EsportsApp.Application/DTOs/Registrations/
- [X] T055 [US4] Implement registration and withdrawal business logic in backend/EsportsApp.Application/Services/TournamentRegistrationService.cs
- [X] T056 [US4] Implement registration endpoints from contracts/tournaments.md in backend/EsportsApp.API/Controllers/TournamentRegistrationsController.cs
- [X] T057 [P] [US4] Add repository queries for registration uniqueness and capacity checks in backend/EsportsApp.Infrastructure/Repositories/TournamentRegistrationRepository.cs
- [X] T058 [US4] Add Angular captain registration/withdraw actions on tournament detail page in frontend/src/app/features/tournaments/tournament-registration-actions.component.ts

**Checkpoint**: User Stories 1 to 4 are independently functional.

---

## Phase 7: User Story 5 - Results and Standings Viewing (Priority: P5)

> Spec: [features/005-results-and-standings/spec.md](features/005-results-and-standings/spec.md)

**Goal**: Public users view tournaments and match history; authenticated users view standings with standard competition ranking and real-time SignalR updates (connection warning + auto-reconnect).

**Independent Test**: Anonymous user sees public tournaments and matches; anonymous user cannot access standings; authenticated user sees standings table with required columns and receives update indicators.

### Tests for User Story 5

- [ ] T059 [P] [US5] Add standings visibility and status-gating unit tests in backend/EsportsApp.Tests/Unit/Application/Services/StandingsQueryServiceTests.cs
- [ ] T060 [P] [US5] Add public listing filter tests: verify Open/InProgress/Completed appear publicly, Draft tournaments do NOT appear to any user other than the owning Organizer (FR-015), and Suspended tournaments appear with Suspended status (FR-012) in backend/EsportsApp.Tests/Unit/Application/Services/TournamentQueryServiceTests.cs

### Implementation for User Story 5

- [X] T061 [P] [US5] Implement standings query DTOs with standard competition ranking (1224 algorithm: position = 1 + count of teams with strictly more points, per FR-031) in backend/EsportsApp.Application/DTOs/Standings/
- [X] T062 [US5] Implement standings and public match query services in backend/EsportsApp.Application/Services/StandingsService.cs
- [X] T063 [US5] Implement standings endpoint from contracts/standings.md in backend/EsportsApp.API/Controllers/StandingsController.cs
- [X] T064 [US5] Build Angular standings table and public match-history components in frontend/src/app/features/matches/
- [X] T065 [US5] Implement Angular SignalR connection warning, visual update indicator, and automatic reconnection after connection loss (FR-028) in frontend/src/app/core/services/signalr.service.ts
- [X] T092 [P] [US5] Add `aria-live="polite"` region (or `role="status"`) to the real-time update indicator component so that assistive technologies announce standings changes without requiring visual focus (FR-028, Constitution §III) in frontend/src/app/features/tournaments/

**Checkpoint**: User Stories 1 to 5 are independently functional.

---

## Phase 8: User Story 6 - Match Result Recording (Priority: P6)

> Spec: [features/006-match-result-recording/spec.md](features/006-match-result-recording/spec.md)

**Goal**: Organizer records/corrects match results, standings recalculate by scoring strategy (full replay), duplicate detection enforced regardless of home/away order, and updates are pushed via SignalR.

**Independent Test**: Organizer records and corrects results in InProgress tournament, standings recalculate correctly for configured scoring system, duplicate match detection works, and non-owner/Player actions return 403.

### Tests for User Story 6

- [ ] T066 [P] [US6] Add match record/correction and duplicate detection unit tests (TR-004) in backend/EsportsApp.Tests/Unit/Application/Services/MatchServiceTests.cs
- [ ] T067 [P] [US6] Add scoring strategy calculation tests (Standard/WTA/Custom) in backend/EsportsApp.Tests/Unit/Application/Scoring/ScoringStrategyTests.cs
- [ ] T068 [P] [US6] Document manual test cases for TR-004 result recording and recalculation in backend/EsportsApp.Tests/Manual/TR-004-match-recording.md

### Implementation for User Story 6

- [X] T069 [P] [US6] Implement scoring strategy interfaces and concrete strategies in backend/EsportsApp.Application/Scoring/
- [X] T070 [US6] Implement match service full-replay standings recalculation in backend/EsportsApp.Application/Services/MatchService.cs
- [X] T071 [US6] Implement match endpoints from contracts/matches.md in backend/EsportsApp.API/Controllers/MatchesController.cs
- [X] T072 [US6] Implement SignalR push payload emission after record/correct in backend/EsportsApp.API/Services/SignalRMatchRealtimeNotifier.cs
- [X] T073 [US6] Build Angular organizer match entry/correction UI in frontend/src/app/features/matches/organizer-match-form.component.ts

**Checkpoint**: All user stories are functional and independently testable.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final hardening, documentation, and end-to-end validation.

- [X] T074 [P] Add admin user management endpoints and policies from contracts/users.md in backend/EsportsApp.API/Controllers/AdminUsersController.cs
- [X] T075 [P] Add admin frontend user/organizer suspension management UI in frontend/src/app/features/admin/
- [X] T076 [P] Add videogame catalog read endpoint from contracts/videogames.md in backend/EsportsApp.API/Controllers/VideogamesController.cs
- [X] T084 [P] Add meta description and single descriptive h1 to public Angular tournament list and detail pages for SEO baseline (Constitution §II) in frontend/src/app/features/tournaments/
- [ ] T085 [P] Create FR-to-unit-test traceability matrix for FR-001 through FR-043 (SC-007): table with FR id, description, and linked test class/method; FRs covered by unit tests must cite the test; FRs covered only by manual tests must be noted as such; file in backend/EsportsApp.Tests/Manual/fr-test-coverage-matrix.md
- [X] T086 [P] Add unit tests verifying HTTP error response mapping 400/409/422/500 from service exceptions (FR-041, FR-042, FR-043) in backend/EsportsApp.Tests/Unit/Application/
- [X] T088 [P] Add unit tests for AdminService suspend/reinstate logic (FR-008) covering: suspend moves non-Completed tournaments to Suspended, reinstate restores pre-suspension status, and role guard prevents non-Admin callers in backend/EsportsApp.Tests/Unit/Application/Services/AdminServiceTests.cs
- [X] T091 [P] Add unit tests for Suspended tournament public-listing visibility: verify Suspended tournaments appear in GET /tournaments response with status=Suspended badge and are excluded from Organizer-advance and registration endpoints (FR-012, features/002-tournament-management/spec.md US5) in backend/EsportsApp.Tests/Unit/Application/Services/TournamentVisibilityTests.cs
- [X] T089 [P] Add unit tests for UserService covering Player own-profile view (FR-009), Organizer allowed-field update rules (FR-037: org name uniqueness and length, password complexity), and GlobalExceptionMiddleware returns 500 with generic body on unhandled exception (FR-040) in backend/EsportsApp.Tests/Unit/Application/Services/UserServiceTests.cs and backend/EsportsApp.Tests/Unit/Middleware/GlobalExceptionMiddlewareTests.cs
- [ ] T077 Improve API request/response examples and auth docs in README.md
- [ ] T078 Add quickstart validation checklist and execution notes in specs/001-esports-tournament-platform/quickstart.md
- [ ] T079 Run backend test suite and capture results summary in backend/EsportsApp.Tests/TestResults/summary.md
- [ ] T080 Run end-to-end manual verification checklist and record outcomes in specs/001-esports-tournament-platform/checklists/e2e-results.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on US1 (Organizer auth) and Phase 2.
- **Phase 5 (US3)**: Depends on US1 (Player auth) and Phase 2.
- **Phase 6 (US4)**: Depends on US2 and US3.
- **Phase 7 (US5)**: Public listing/match-history scaffolding can start after US2. Live standings and real-time UX (FR-028, FR-039) require Phase 8 (US6) to be complete — finalize Phase 7 standings and SignalR components only after Phase 8 is done.
- **Phase 8 (US6)**: Depends on US2 and US4. Should be implemented before Phase 7 standings components are finalized.
- **Phase 9 (Polish)**: Depends on all selected user stories.

### User Story Dependency Graph

- **US1 (P1)**: Foundation auth/profile story; prerequisite for role-protected stories.
- **US2 (P2)**: Requires US1.
- **US3 (P3)**: Requires US1.
- **US4 (P4)**: Requires US2 + US3.
- **US6 (P6)**: Requires US2 + US4.
- **US5 (P5)**: Public listing depends on US2. Live standings and real-time features (FR-028, FR-039) require US6 — implement US6 before finalizing US5 standings and SignalR components.

### Within Each User Story

- Tests first (unit + manual docs when required).
- DTO/validation before service logic.
- Service logic before controller endpoints.
- Backend API before frontend integration.
- Complete and validate each story independently before advancing.

---

## Parallel Opportunities

- Setup: T003-T007 can run in parallel after T001-T002.
- Foundational: T012, T013, T017, T018, T019, T021 can run in parallel once T008-T011 are in place.
- US1: T022-T025, T093 and T026-T027 can run in parallel; frontend T031 can proceed after API contracts are stable.
- US2: T034-T036 and T037/T040/T041 can run in parallel.
- US3: T043-T045 and T046/T049/T050 can run in parallel; T090 (auto-withdrawal) runs after T047 (TeamService) is in place.
- US4: T052-T054/T057 can run in parallel.
- US5: T059-T061/T064 and T092 can run in parallel.
- US6: T066-T069 can run in parallel before T070-T073 integration tasks.
- Polish: T074-T076, T084-T086, T088-T091 can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Execute US1 tests in parallel:
T022, T023, T024, T025

# Execute US1 implementation in parallel where safe:
T026, T027, T031
```

## Parallel Example: User Story 2

```bash
# Execute US2 tests/docs in parallel:
T034, T035, T036

# Execute US2 implementation in parallel where safe:
T037, T040, T041
```

## Parallel Example: User Story 3

```bash
# Execute US3 tests in parallel:
T043, T044, T045

# Execute US3 implementation in parallel where safe:
T046, T049, T050
```

## Parallel Example: User Story 4

```bash
# Execute US4 tests/docs in parallel:
T052, T053

# Execute US4 implementation in parallel where safe:
T054, T057
```

## Parallel Example: User Story 5

```bash
# Execute US5 tests in parallel:
T059, T060

# Execute US5 implementation in parallel where safe:
T061, T064
```

## Parallel Example: User Story 6

```bash
# Execute US6 tests/docs in parallel:
T066, T067, T068

# Execute US6 implementation in parallel where safe:
T069, then integrate with T070-T073
```

---

## Implementation Strategy

### Feature Spec Reference

Each delivery phase maps directly to a per-feature spec file under [features/](features/). Consult that file for the full acceptance scenario list, independent test instructions, and exact FR references before implementing the phase.

### MVP First (User Story 1 Only)

1. Complete Phase 1 (Setup).
2. Complete Phase 2 (Foundational).
3. Complete Phase 3 (US1).
4. Validate US1 independently using T022-T025 acceptance tests.
5. Demo authentication and profile update flows.

### Incremental Delivery

1. Deliver US1 (auth/profile).
2. Deliver US2 (tournament management).
3. Deliver US3 (team management).
4. Deliver US4 (tournament registration).
5. Deliver US6 (match recording and recalculation).
6. Deliver US5 (public results/standings UX with real-time behavior).
7. Finish with Phase 9 polish.

### Parallel Team Strategy

1. Team-wide: Phase 1 and 2.
2. Stream A: US2 while Stream B works on US3 (after US1).
3. Stream C: Start US5 UI scaffolding after US2 contracts stabilize.
4. Merge on US4 and US6 integration.
5. Close with cross-cutting polish and full validation.

---

## Notes

- All task IDs are sequential from T001 to T086.
- All user story tasks include a `[USx]` label.
- All tasks include explicit file paths.
- Tests are included because the specification explicitly requires automated and manual testing coverage.
