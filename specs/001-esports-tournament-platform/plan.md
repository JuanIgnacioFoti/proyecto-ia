# Implementation Plan: Esports Tournament Management Platform

**Branch**: `001-esports-tournament-platform` | **Date**: 2026-04-20 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/001-esports-tournament-platform/spec.md`  
**Last Updated**: 2026-04-24 — per-feature specs added under `features/`

## Summary

Full-stack web platform for organizing and managing esports league (round-robin) tournaments. Backend: .NET 10 ASP.NET Core Web API + EF Core + SQL Server, JWT auth, SignalR for real-time standings, strategy pattern for scoring, and DTO-based request/response contracts so domain entities are not exposed directly by the API. Frontend: Angular 21 SPA with SignalR client. Architecture enforces strict 4-layer Clean Architecture: API → Application → Domain ← Infrastructure, every dependency through an interface. System functionalities are managed as GitHub Issues; each PR must reference and close its linked issue via `Closes #N`.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (backend), TypeScript / Angular 21 (frontend)  
**Primary Dependencies**: ASP.NET Core Web API, EF Core 10, Microsoft.AspNetCore.SignalR, Microsoft.AspNetCore.Authentication.JwtBearer, Swashbuckle (Swagger), MSTest, Moq; Angular 21, @microsoft/signalr, Angular Router, Angular HTTP Client  
**Storage**: SQL Server (EF Core code-first migrations; seed data for Videogame catalog and Admin account)  
**Testing**: MSTest + Moq (unit tests for TR-001 to TR-004); documented manual test cases in specs/  
**Target Platform**: Web server; Docker-composable (Kestrel + Angular dev/static + SQL Server container)  
**Project Type**: Web application (backend REST API + Angular SPA frontend)  
**Performance Goals**: Standings/match updates delivered within 5 seconds of recording (SC-004); user registration UX under 2 minutes (SC-001)  
**Constraints**: JWT 24-hour expiry (FR-002); scoring system locked once InProgress (FR-032); real-time via SignalR only — no polling; no secrets committed; all inputs validated server-side  
**Scale/Scope**: Course-grade project; multi-user concurrent access; league format only; fixed videogame catalog; Admin bootstrapped from environment at startup

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status | Notes |
|------|--------|-------|
| Dynamic Web App (Principle I) | ✅ PASS | Full-stack SPA + REST API; deployable via Docker Compose |
| Semantic HTML + Progressive Enhancement (Principle II) | ✅ PASS | Angular renders semantic HTML; public pages (tournament list, match history, FR-015) accessible without JavaScript. **Scope note**: the standings table (FR-039) requires authentication by design — it is intentionally gated and is exempt from the first-paint requirement; this is not a Progressive Enhancement violation. |
| Accessibility Baseline (Principle III) | ✅ PASS | All Angular form controls must have labels; keyboard-operable interactive elements; no color-only information |
| Fast by Default (Principle IV) | ✅ PASS | Angular lazy-loaded feature modules; no unnecessary third-party JS |
| Simplicity & Maintainability (Principle V) | ✅ PASS | Strategy + Repository patterns are directly required by FR-033 and the spec's layer separation assumptions |
| Deployability | ✅ PASS | Docker Compose brings up SQL Server + backend + Angular with one command |
| No secrets committed | ✅ PASS | All credentials via environment variables; `.env` files gitignored |
| Entry HTML | ✅ PASS | Angular `index.html` must include `lang`, `meta charset`, `meta viewport`, descriptive `title` |
| Safety | ✅ PASS | No `eval`; all inputs validated on server; external data treated as untrusted |
| SEO baseline (public pages) | ✅ PASS | Tournament listing page must include `meta description` + single descriptive `h1` |

**Post-Phase 1 re-check**: All gates still pass — no new dependencies or patterns introduced that would violate any principle.

## Project Structure

### Documentation (this feature)

```text
specs/001-esports-tournament-platform/
├── plan.md              # This file
├── spec.md              # Master consolidated spec (all features)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
├── contracts/
│   ├── auth.md          # Login, Player & Organizer registration
│   ├── users.md         # Profile view and update
│   ├── tournaments.md   # Tournament CRUD and lifecycle
│   ├── teams.md         # Team management and invitations
│   ├── matches.md       # Match result recording and correction
│   ├── standings.md     # Standings retrieval
│   └── videogames.md    # Videogame catalog listing
├── checklists/
│   ├── api.md
│   ├── data-model.md
│   ├── requirements.md
│   ├── security.md
│   ├── tasks.md
│   └── ux.md
└── features/            # Per-feature specs (added 2026-04-24)
    ├── 001-registration-and-login/spec.md
    ├── 002-tournament-management/spec.md
    ├── 003-team-formation/spec.md
    ├── 004-tournament-registration/spec.md
    ├── 005-results-and-standings/spec.md
    └── 006-match-result-recording/spec.md
```

### Per-Feature Spec Index

Each file in `features/` is a self-contained specification scoped to one deliverable unit, independently testable, with its own user stories, acceptance scenarios, functional requirements (FR-NNN references), key entities, success criteria, and assumptions.

| Feature | Priority | Key FRs | Spec File |
|---------|----------|---------|-----------|
| 001 — Registration & Login | P1 | FR-001 to FR-009, FR-036 to FR-038 | [features/001-registration-and-login/spec.md](features/001-registration-and-login/spec.md) |
| 002 — Tournament Management | P2 | FR-010 to FR-012, FR-014 to FR-015, FR-029 to FR-033 | [features/002-tournament-management/spec.md](features/002-tournament-management/spec.md) |
| 003 — Team Formation | P3 | FR-016 to FR-021 | [features/003-team-formation/spec.md](features/003-team-formation/spec.md) |
| 004 — Tournament Registration | P4 | FR-022 to FR-024 | [features/004-tournament-registration/spec.md](features/004-tournament-registration/spec.md) |
| 005 — Results & Standings Viewing | P5 | FR-015, FR-028, FR-031, FR-039 | [features/005-results-and-standings/spec.md](features/005-results-and-standings/spec.md) |
| 006 — Match Result Recording | P6 | FR-025 to FR-028, FR-035 | [features/006-match-result-recording/spec.md](features/006-match-result-recording/spec.md) |

### Source Code (repository root)

```text
backend/
├── EsportsApp.API/            # ASP.NET Core Web API (controllers, middleware, hubs, DI wiring)
│   ├── Controllers/           # AuthController, UsersController, TournamentsController,
│   │                          #   TeamsController, MatchesController, StandingsController,
│   │                          #   TournamentRegistrationsController, VideogamesController,
│   │                          #   AdminController
│   ├── Hubs/                  # TournamentHub.cs (SignalR)
│   ├── Middleware/            # GlobalExceptionMiddleware.cs
│   ├── Services/              # SignalRMatchRealtimeNotifier.cs
│   └── Program.cs
├── EsportsApp.Application/    # Use-case services, DTOs, interfaces, exceptions
│   ├── Services/              # AuthService, TournamentService, TeamService, MatchService, …
│   ├── DTOs/                  # Request/Response DTOs per feature group
│   ├── Interfaces/            # IAuthService, ITournamentService, ITeamService, …
│   ├── Scoring/               # IScoringStrategy, StandardScoringStrategy,
│   │                          #   WinnerTakesAllScoringStrategy, CustomScoringStrategy,
│   │                          #   ScoringStrategyFactory
│   └── Exceptions/            # AppExceptions.cs (domain + validation exceptions)
├── EsportsApp.Domain/         # Entities, enums, value objects — no framework dependencies
│   ├── Entities/
│   └── Enums/
├── EsportsApp.Infrastructure/ # EF Core, repositories, migrations
│   ├── Data/                  # AppDbContext, seed data
│   ├── Migrations/
│   └── Repositories/         # IRepository<T> implementations
└── EsportsApp.Tests/          # MSTest unit test project
    ├── AuthServiceTests.cs
    ├── TournamentServiceTests.cs
    ├── ScoringStrategyTests.cs
    └── (additional test files per TR-001 to TR-004)

frontend/
├── src/
│   ├── app/
│   │   ├── core/              # AuthService, JwtInterceptor, SignalRService, guards
│   │   ├── features/
│   │   │   ├── auth/          # Login, Player registration, Organizer registration pages
│   │   │   ├── tournaments/   # Tournament list, detail, create/edit, standings, match history
│   │   │   ├── teams/         # Team creation, roster, invitations
│   │   │   └── admin/         # Admin user management panel
│   │   └── shared/            # Shared UI components (navbar, badges, notification banner)
│   ├── environments/
│   └── index.html             # lang, meta charset, meta viewport, descriptive title
├── nginx.conf
└── Dockerfile
```

**Structure Decision**: Web application. Backend uses a 4-project Clean Architecture solution; frontend is an Angular 21 SPA served by Nginx in production.

---

## Phase 0: Research (Resolved)

> Full findings in [research.md](research.md)

| # | Unknown / Technology | Decision | Reference |
|---|----------------------|----------|-----------|
| 1 | .NET 10 Web API style | Controller-based (not Minimal API) — better suited to RBAC attributes and many endpoint groups | research.md §1 |
| 2 | JWT authentication | `Microsoft.AspNetCore.Authentication.JwtBearer`; symmetric HMAC-SHA256; 24 h expiry; secret from `JWT_SECRET` env var | research.md §2 |
| 3 | TeamCaptain role RBAC | Ownership verified at service layer (check `Team.CaptainId`); NOT a JWT claim — avoids token re-issue on team creation/loss | research.md §3 |
| 4 | SignalR real-time standings | Strongly-typed hub `TournamentHub`; clients join group `tournament-{id}`; `MatchService` pushes after every recalculation | research.md §4 |
| 5 | Scoring system pattern | Strategy pattern: `IScoringStrategy` with three implementations; resolved via `ScoringStrategyFactory` | research.md §5 |
| 6 | Standings recalculation | Full replay on every record/correction — load all matches, reset standings, iterate, persist, push | research.md §6 |
| 7 | EF Core + SQL Server | Code-first migrations; `HasData` seeding for Videogame catalog and Admin account | research.md §7 |
| 8 | Unit testing | MSTest + Moq; mocked repositories per service; TR-001 to TR-004 coverage | research.md §8 |
| 9 | GitHub Issues workflow | Each user story = one issue; every PR includes `Closes #N`; auto-linked and auto-closed on merge | research.md §9 |
| 10 | Invitation expiry | Lazy evaluation at acceptance time: `CreatedAt + 7 days < UtcNow`; no background job | research.md §10 |
| 11 | Admin bootstrap | Seeded from `ADMIN_EMAIL` / `ADMIN_PASSWORD` env vars; BCrypt hash computed at startup if row absent | research.md §11 |
| 12 | Angular 21 architecture | Standalone components; lazy-loaded feature routes; `HttpClient` + `@microsoft/signalr`; core providers for auth/guards/SignalR | research.md §12 |

---

## Phase 1: Design (Complete)

### Data Model

> Full entity definitions, constraints, and state diagrams in [data-model.md](data-model.md)

**Core Entities**:

| Entity | Key Fields | Notable Constraints |
|--------|-----------|---------------------|
| `User` | `Id`, `Email`, `PasswordHash`, `Role`, `IsActive` | Email UNIQUE; `IsActive=false` = suspended |
| `Player` | `Id` (FK User), `Username`, `RealName`, `MainVideogameId` | Username UNIQUE, immutable |
| `Organizer` | `Id` (FK User), `OrganizationName` | OrgName UNIQUE, 3–60 chars |
| `Videogame` | `Id`, `Name` | Seeded at startup; immutable at runtime |
| `Team` | `Id`, `Name`, `VideogameId`, `CaptainId` | `UNIQUE(Name, VideogameId)`; Captain ≤ 1 team/game |
| `TeamMember` | `TeamId`, `PlayerId` | `UNIQUE(TeamId, PlayerId)`; Player ≤ 1 team/game |
| `TeamInvitation` | `TeamId`, `InvitedPlayerId`, `Status`, `CreatedAt` | Expires lazily at 7 days |
| `Tournament` | `Id`, `Name`, `VideogameId`, `OrganizerId`, `Status`, `PreSuspensionStatus` | 5 lifecycle states; forward-only Organizer transitions |
| `ScoringSystem` | `TournamentId` (UNIQUE), `Type`, `WinPoints`, `DrawPoints`, `LossPoints` | Immutable once InProgress; WinPts ≥ DrawPts ≥ LossPts ≥ 0 |
| `TournamentRegistration` | `TournamentId`, `TeamId`, `Status` | `UNIQUE(TournamentId, TeamId)` |
| `Match` | `TournamentId`, `HomeTeamId`, `AwayTeamId`, `HomeScore`, `AwayScore`, `PlayedAt` | Duplicate detection ignores home/away order |
| `Standing` | `TournamentId`, `TeamId`, `Points`, `MatchesPlayed`, `Wins`, `Draws`, `Losses` | `UNIQUE(TournamentId, TeamId)`; position computed at query time |

**Enums**: `UserRole` (Player, Organizer, Admin) · `TournamentStatus` (Draft, Open, InProgress, Completed, Suspended) · `ScoringSystemType` (Standard, WinnerTakesAll, Custom) · `InvitationStatus` (Pending, Accepted, Declined, Expired) · `RegistrationStatus` (Active, Withdrawn)

### API Contracts

> Full request/response schemas in [contracts/](contracts/)

| Contract File | Endpoints Covered |
|---------------|------------------|
| [contracts/auth.md](contracts/auth.md) | POST /auth/register/player, POST /auth/register/organizer, POST /auth/login |
| [contracts/users.md](contracts/users.md) | GET /users/me, PUT /users/me |
| [contracts/tournaments.md](contracts/tournaments.md) | CRUD + lifecycle advance (GET, POST, PUT, PATCH /tournaments/{id}/status) |
| [contracts/teams.md](contracts/teams.md) | Team CRUD, invitations, member removal, captaincy transfer |
| [contracts/matches.md](contracts/matches.md) | POST /tournaments/{id}/matches, PUT /tournaments/{id}/matches/{matchId} |
| [contracts/standings.md](contracts/standings.md) | GET /tournaments/{id}/standings |
| [contracts/videogames.md](contracts/videogames.md) | GET /videogames |

### Scoring Strategy Design

```
IScoringStrategy
  ├── StandardScoringStrategy         (3 / 1 / 0)
  ├── WinnerTakesAllScoringStrategy   (3 / 0 / 0)
  └── CustomScoringStrategy           (Organizer-defined)

ScoringStrategyFactory.Resolve(ScoringSystemType) → IScoringStrategy
IScoringStrategy.Calculate(homeScore, awayScore, config) → (homePoints, awayPoints)
```

Open/closed for extension: new types require only a new class implementing `IScoringStrategy` — no existing code modified (FR-033).

### SignalR Real-Time Flow

```
Organizer records/corrects result
  → MatchService.RecordResultAsync()
    → Full standings replay
    → Persist updated Standing rows
    → IHubContext<TournamentHub>.Clients.Group("tournament-{id}")
        .SendAsync("ReceiveStandingsUpdate", StandingsUpdateDto)
          → Angular SignalRService emits update$
            → TournamentDetailComponent updates standings + match list in place
            → Brief highlight animation shown
```

---

## Feature Delivery Sequence

The six features should be implemented in priority order. Each is independently deployable.

| Order | Feature | Depends On | Deliverable |
|-------|---------|------------|-------------|
| 1 | [Registration & Login](features/001-registration-and-login/spec.md) | — | Working auth with JWT; RBAC on all endpoints |
| 2 | [Tournament Management](features/002-tournament-management/spec.md) | Feature 1 (auth) | Organizer can create, edit, advance tournaments |
| 3 | [Team Formation](features/003-team-formation/spec.md) | Feature 1 (auth) | Players can form teams and invite members |
| 4 | [Tournament Registration](features/004-tournament-registration/spec.md) | Features 2 + 3 | Captains can register/withdraw teams |
| 5 | [Results & Standings Viewing](features/005-results-and-standings/spec.md) | Features 2 + 4 (public scaffold); finalize standings + SignalR components after Feature 6 | Public browsing; authenticated standings; SignalR wiring |
| 6 | [Match Result Recording](features/006-match-result-recording/spec.md) | Features 2 + 4 + 5 | Organizer records/corrects results; standings auto-update |

---

## Testing Strategy

| Requirement | Type | Coverage Target | Feature Spec |
|-------------|------|-----------------|-------------|
| TR-001 — Player Account Registration | Unit (MSTest + Moq) + Manual | Username uniqueness, email format/uniqueness, required fields, password rules | [001-registration-and-login](features/001-registration-and-login/spec.md) |
| TR-002 — Tournament Creation | Unit (MSTest + Moq) + Manual | Future start date, end > start, max teams ≥ 2, min members ≥ 1, scoring constraints | [002-tournament-management](features/002-tournament-management/spec.md) |
| TR-003 — Tournament Registration | Unit (MSTest + Moq) + Manual | Tournament status, game match, slots, min members, duplicate | [004-tournament-registration](features/004-tournament-registration/spec.md) |
| TR-004 — Match Result Recording | Unit (MSTest + Moq) + Manual | Score recording, win/draw/loss derivation, standings recalc, correction, duplicate detection | [006-match-result-recording](features/006-match-result-recording/spec.md) |

---

## Error Handling Contract

All exceptions are handled by `GlobalExceptionMiddleware`:

| Exception Type | HTTP Status | Response Body |
|----------------|-------------|---------------|
| Validation error (input/constraint) | 400 Bad Request | `{ errors: { field: [messages] } }` |
| Duplicate / uniqueness violation | 409 Conflict | `{ error: "descriptive message" }` |
| Domain rule violation | 422 Unprocessable Entity | `{ error: "descriptive message" }` |
| Authorization failure | 403 Forbidden | `{ error: "Forbidden" }` |
| Not found | 404 Not Found | `{ error: "Resource not found" }` |
| Unhandled exception | 500 Internal Server Error | `{ error: "An unexpected error occurred" }` (no stack trace) |

---

## Quickstart

> Full setup instructions in [quickstart.md](quickstart.md)

```bash
# 1. Set environment variables (.env in backend/EsportsApp.API/)
# 2. Start full stack
docker compose up
# API: http://localhost:5000  •  Frontend: http://localhost:4200  •  Swagger: http://localhost:5000/swagger
```

---

## Open Questions / Risks

| # | Item | Resolution |
|---|------|------------|
| 1 | Angular 21 standalone vs. NgModule | Use standalone components (Angular 21 default); no NgModule needed for feature components |
| 2 | JWT secret rotation | Out of scope for course project; documented as known limitation |
| 3 | Match scheduling (auto-generate round-robin schedule) | Out of scope; Organizer records results ad-hoc; no schedule enforced |
| 4 | Organizer email change | Locked after registration (FR-037); documented as intentional constraint |
