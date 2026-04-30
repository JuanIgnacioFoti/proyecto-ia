---
description: "Implementation tasks for Esports Tournament Platform feature"
---

# Tasks: Esports Tournament Platform

**Input**: `specs/001-esports-tournament-platform/spec.md`
**Prerequisites**: `specs/001-esports-tournament-platform/` spec.md, plan.md (when ready), data-model.md (when ready), contracts/

**Tests**: Unit tests required for features marked with (*) in the spec: tournament creation, match result registration, player account registration, tournament enrollment. Use MSTest + Moq.

**Organization**: Tasks are grouped by phase and user story so each story can be implemented and tested independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)

---

## Phase 1: Setup (Shared Infrastructure)

Purpose: Project initialization and basic structure

- [ ] T001 Create repository structure
  - Backend: `backend/` (ASP.NET minimal API or Web API)
  - Frontend: `frontend/` (Angular workspace)
  - Database: `infra/db/` (migrations folder)
  - Tests: `backend/tests/`, `frontend/tests/`
  - Docs: `docs/` (design docs, quickstart, context files)

- [ ] T002 Initialize backend (.NET 10) project
  - `backend/src/Project.Api` (Web API)
  - `backend/src/Project.Domain` (domain models)
  - `backend/src/Project.Application` (services/use-cases)
  - `backend/src/Project.Infrastructure` (EF Core, repositories)
  - `backend/tests/Project.UnitTests` (MSTest)

- [ ] T003 Initialize frontend (Angular 21) workspace
  - `frontend/` Angular app with strict TypeScript settings

- [ ] T004 Configure CI basics (GitHub Actions) with simple build for backend and frontend

- [ ] T005 Configure linting/formatting
  - Backend: EditorConfig, dotnet-format
  - Frontend: ESLint, Prettier

---

## Phase 2: Foundational (Blocking Prerequisites)

Purpose: Core infrastructure that MUST be complete before ANY user story can be implemented

- [ ] T006 Setup SQL Server local dev container or instructions in `infra/db/` (scripts for migrations)
- [ ] T007 [P] Implement authentication/authorization framework
  - Create `Auth` module in backend
  - Define JWT tokens and role-based authorization (Organizer, Player)
  - Implement middleware for 401/403 handling
  - Add sample seeded admin/organizer/player accounts for local dev

- [ ] T008 [P] Setup API routing and middleware
  - Centralized exception handler returning structured error responses (400/401/403/500)
  - Logging configuration

- [ ] T009 Create base domain models and EF Core mappings
  - Entities: User, Organizer, Player, Team, TeamInvitation, Tournament, Match, Standings, ScoringSystem
  - DB migrations and initial schema

- [ ] T010 [P] Implement repository interfaces and a basic in-memory implementation for early testing

- [ ] T011 Configure environment configuration and secrets handling (appsettings.json with profiles)

- [ ] T012 [P] Setup testing infrastructure (MSTest + Moq) and CI test job

Checkpoint: Foundation ready - user story implementation can begin

---

## Phase 3: User Story US1 - Organizer Registration and Tournament Creation (Priority: P1) 🎯

Goal: Allow organizers to register, authenticate, and create tournaments (draft state)

**Independent Test**: Register organizer, create tournament with valid dates and settings, verify draft visibility

### Tests (Write tests first)
- [ ] T013 [P] [US1] Unit tests for organizer registration validation in `backend/tests/Project.UnitTests/OrganizerRegistrationTests.cs`
- [ ] T014 [P] [US1] Integration test for tournament creation flow in `backend/tests/Project.IntegrationTests/TournamentCreationTests.cs`

### Implementation
- [ ] T015 [US1] Implement Organizer registration endpoint: `POST /api/organizers/register` (backend/src/Project.Api/Controllers/OrganizersController.cs)
- [ ] T016 [US1] Implement Organizer account update endpoint: `PUT /api/organizers/{id}`
- [ ] T017 [US1] Implement Tournament CRUD: `backend/src/Project.Api/Controllers/TournamentsController.cs`
- [ ] T018 [US1] Implement validation: start date future, end after start, maxTeams >=2
- [ ] T019 [US1] Add unit tests for tournament validation in `TournamentValidationTests.cs`

Checkpoint: US1 should be demoable independently

---

## Phase 4: User Story US2 - Player Registration and Team Formation (Priority: P1)

Goal: Allow players to register, create teams, invite members, accept invites

**Independent Test**: Create player, create team, invite player, accept invite

### Tests
- [ ] T020 [P] [US2] Unit tests for player registration and uniqueness checks
- [ ] T021 [P] [US2] Integration test for team creation and invitation flow

### Implementation
- [ ] T022 [US2] Implement Player registration endpoint: `POST /api/players/register`
- [ ] T023 [US2] Implement Team creation endpoint: `POST /api/teams`
- [ ] T024 [US2] Implement Team invitation endpoints: `POST /api/teams/{teamId}/invite`, `POST /api/teams/{teamId}/invitations/{invitationId}/accept`
- [ ] T025 [US2] Enforce captain and one-team-per-game rules

Checkpoint: US2 should be demoable independently

---

## Phase 5: User Story US3 - Team Tournament Enrollment (Priority: P1)

Goal: Allow captains to enroll teams into open tournaments with validation

### Tests
- [ ] T026 [P] [US3] Unit tests for enrollment validation (capacity, min players, duplicate)
- [ ] T027 [P] [US3] Integration test for enrollment flow

### Implementation
- [ ] T028 [US3] Enrollment endpoint: `POST /api/tournaments/{tournamentId}/enrollments` (only captain)
- [ ] T029 [US3] Enrollment validation logic
- [ ] T030 [US3] Update tournament participant list and check capacity

Checkpoint: US3 demoable independently

---

## Phase 6: User Story US4 - Match Results & Standings (Priority: P2)

Goal: Organizers register match results and system recalculates standings using configured scoring system

### Tests
- [ ] T031 [P] [US4] Unit tests for scoring strategies: Standard, WTA, Custom
- [ ] T032 [P] [US4] Integration test for result registration updating standings

### Implementation
- [ ] T033 [US4] Implement Match result registration endpoint: `POST /api/tournaments/{id}/matches`
- [ ] T034 [US4] Implement ScoringStrategy interface and three implementations
- [ ] T035 [US4] Implement standings recalculation service
- [ ] T036 [US4] Prevent duplicate matches (same teams and datetime)

Checkpoint: Standings update verified via tests

---

## Phase 7: US5-US7 and Polish (P2-P3)

- [ ] T037 [US5] Implement standings viewing endpoint: `GET /api/tournaments/{id}/standings`
- [ ] T038 [US6] Enforce state transitions and restrictions
- [ ] T039 [US7] Implement account modification endpoints
- [ ] T040 Cross-cutting: Centralized exception handling and error format
- [ ] T041 Cross-cutting: Logging and observability (structured logs)
- [ ] T042 Polish: Input validation messages in Spanish, i18n baseline
- [ ] T043 Documentation: `docs/` include quickstart, architecture diagram, Agent Skill instructions

---

## Phase 8: Agent Skill & IA Process

- [ ] T044 Design Agent Skill (purpose: code reviewer for SOLID/Clean Code rules)
  - Define skill prompts and acceptance criteria
  - Create 3 test prompts and expected outputs
- [ ] T045 Implement minimal Agent Skill as scripts in `.github/agents/` or `.specify/extensions/`
- [ ] T046 Document Diary of Prompts in `docs/diary-of-prompts.md`

---

## Phase 9: Testing & Delivery

- [ ] T047 Run full test suite in CI
- [ ] T048 Prepare Document 1, Document 2, Document 3 PDFs in `docs/` for submission
- [ ] T049 Record demo video and add link to Document 1

---

## Dependencies & Execution Order

- Phase 1 → Phase 2 (blocking) → User Stories P1 → P2 → P3
- Parallelize tasks marked [P] where possible

---

## Notes

- Tests for features marked (*) are required (refer to spec.md)
- Use Strategy Pattern for scoring systems to ensure extensibility
- Ensure all endpoints return 401 for unauthenticated and 403 for unauthorized
- Use migrations for DB schema changes

---

*Generated from `specs/001-esports-tournament-platform/spec.md` on 2026-04-30*
