# Research: Esports Tournament Management Platform

**Phase**: 0 | **Date**: 2026-04-20 | **Spec**: [spec.md](spec.md)

All NEEDS CLARIFICATION items from Technical Context are resolved below.

---

## 1. .NET 10 + ASP.NET Core Web API Setup

**Decision**: Use ASP.NET Core controller-based Web API (not Minimal API). Each layer is a separate class library project referenced by the API project. `Program.cs` wires up all services, authentication, SignalR, Swagger, and the global exception middleware.

**Rationale**: Controller-based API is better suited to RBAC (`[Authorize(Roles = "...")]` attributes) and the number of endpoints (12+ resource groups). Minimal API would produce verbose, hard-to-read `Program.cs` at this scale.

**Alternatives Considered**:
- Minimal API: rejected — attribute-based auth and grouped controllers read more cleanly at this feature count.

---

## 2. JWT Authentication — .NET 10

**Decision**: Use `Microsoft.AspNetCore.Authentication.JwtBearer`. Configure symmetric key signing via `SecurityAlgorithms.HmacSha256`. Token claims include: `sub` (userId), `email`, `role` (UserRole enum value). Token lifetime = 24 hours (FR-002). Secret loaded from environment variable `JWT_SECRET` (minimum 32 characters). `[Authorize]` and `[Authorize(Roles = "Organizer")]` used on controller actions.

**Rationale**: Native .NET support; zero additional packages; environment-variable secret satisfies "No secrets" constitution gate.

**Alternatives Considered**:
- Cookie-based sessions: rejected — spec explicitly requires JWT.
- OAuth2 / external IdP: rejected — scope is self-registration only with no external identity provider.

---

## 3. Role-Based Access Control (RBAC) — TeamCaptain Role

**Decision**: The `TeamCaptain` role is **not stored as a static JWT claim**. Instead, ownership is verified at the service layer on each request: the service checks whether the authenticated Player is the `CaptainId` of the relevant Team in the database. This avoids the need to re-issue tokens when a Player creates or loses a team.

**Rationale**: Captain status is per-team and per-videogame — it cannot be a single global claim. Database ownership check is simple and avoids token invalidation complexity.

**Alternatives Considered**:
- Re-issuing JWT on team creation: rejected — forces re-authentication; poor UX.
- Storing Captain claim as a list of team IDs in JWT: rejected — tokens grow unbounded; stale on team changes.

---

## 4. SignalR Real-Time Standings

**Decision**:
- Backend: strongly-typed hub `TournamentHub : Hub<IStandingsClient>` with interface method `ReceiveStandingsUpdate(StandingsUpdateDto payload)`. Clients join a SignalR group named `tournament-{tournamentId}` on connection. The `MatchService` injects `IHubContext<TournamentHub, IStandingsClient>` and calls the group after every standings recalculation.
- Frontend: `@microsoft/signalr` `HubConnectionBuilder` in an Angular `SignalRService`. Feature components subscribe to `ReceiveStandingsUpdate` and update the standings and match list in place.

**Rationale**: Strongly-typed hubs prevent method name mismatches. Tournament-scoped groups limit push traffic to relevant subscribers only.

**Alternatives Considered**:
- Polling: explicitly rejected by FR-028.
- Server-Sent Events: rejected — SignalR specified in spec clarifications.
- Broadcast to all clients: rejected — unnecessary traffic; group-per-tournament is cleaner.

---

## 5. Scoring System — Strategy Pattern

**Decision**:
```
IScoringStrategy
  ├── StandardScoringStrategy      (3 / 1 / 0)
  ├── WinnerTakesAllScoringStrategy (3 / 0 / 0)
  └── CustomScoringStrategy         (Organizer-defined)
```
`ScoringStrategyFactory.Resolve(ScoringSystemType type)` returns the correct strategy. `IScoringStrategy.Calculate(int homeScore, int awayScore, ScoringSystemConfig config)` returns `(int homePoints, int awayPoints)`. Standings recalculation calls the factory — no existing strategy code changes when a new type is added.

**Rationale**: Directly satisfies FR-033 (open for extension, closed for modification).

**Alternatives Considered**:
- Switch statement in service: rejected — violates FR-033.
- Enum-indexed lookup table: rejected — cannot encapsulate different calculation logic per type.

---

## 6. Standings Recalculation Algorithm

**Decision**: Full replay on every match record/correction.
1. Load all `Match` rows for the tournament.
2. Reset all `Standing` rows for the tournament to zero.
3. Iterate every match, call `IScoringStrategy.Calculate`, accumulate into in-memory dictionary.
4. Persist updated `Standing` rows.
5. Push `ReceiveStandingsUpdate` via SignalR group.

**Rationale**: Full replay guarantees correctness when past results are corrected (FR-027). At course-grade scale (max teams typically ≤ 20), replay of all matches is negligible.

**Alternatives Considered**:
- Incremental delta update: rejected — requires reversal logic on correction; error-prone.
- SQL materialized view: rejected — business logic must remain in application layer per spec assumptions.

---

## 7. EF Core Code-First + SQL Server

**Decision**:
- `AppDbContext` in `EsportsApp.Infrastructure.Data` with `DbSet<T>` for all entities.
- Migrations: `dotnet ef migrations add` / `dotnet ef database update`.
- Seed data (Videogame catalog + Admin user) via `modelBuilder.Entity<T>().HasData(...)` in `OnModelCreating`, with Admin credentials from environment variables.
- Repository pattern: interfaces in `EsportsApp.Infrastructure.Interfaces`, EF Core implementations in `EsportsApp.Infrastructure.Repositories`.

**Rationale**: Code-first gives full schema control; `HasData` seed is reproducible on fresh deployments. Repository interfaces allow full Moq mocking in unit tests.

**Alternatives Considered**:
- Database-first: rejected — spec defines entities; code-first is simpler for greenfield projects.
- Dapper: rejected — EF Core LINQ is sufficient and reduces boilerplate for this entity complexity.

---

## 8. Unit Testing — MSTest + Moq

**Decision**: Each service class under test (`PlayerService`, `TournamentService`, `MatchService`, `TeamService`) receives mocked repository interfaces via `Moq`. Tests follow the Arrange-Act-Assert pattern. Key test targets per TR-001 to TR-004:

| Test Requirement | Service Method | Key Scenarios |
|-----------------|---------------|---------------|
| TR-001 (Player Registration) | `PlayerService.RegisterAsync` | username uniqueness, email format + uniqueness, required fields |
| TR-002 (Tournament Creation) | `TournamentService.CreateAsync` | future start date, end > start, max teams ≥ 2, custom scoring constraints |
| TR-003 (Tournament Registration) | `TournamentService.RegisterTeamAsync` | tournament Open, game match, slot available, min members, duplicate |
| TR-004 (Match Result Recording) | `MatchService.RecordResultAsync` | score recording, win/draw/loss derivation, standings recalc per scoring system, correction, duplicate detection |

**Alternatives Considered**:
- xUnit: available in .NET but not specified by spec.
- Integration tests with in-memory DB: complementary but not the primary requirement.

---

## 9. GitHub Issues & PR Linkage Workflow

**Decision**: Each user story / functional requirement group becomes a **GitHub Issue** (created before or during implementation). Every PR that implements functionality for an issue must include `Closes #N` or `Resolves #N` in the PR description body. GitHub automatically links the PR to the issue and closes the issue when the PR is merged to the default branch.

**Issue creation**: Issues are created using the GitHub web UI or CLI (`gh issue create`) at the start of each implementation sprint. Issue titles match the scope column in plan.md's GitHub Issues table.

**Rationale**: Direct requirement from project guidelines: "Las funcionalidades del sistema deben gestionarse como issues en el repositorio. Cada issue debe estar vinculado al PR que lo resuelve."

**Alternatives Considered**:
- Milestones only: rejected — issues provide finer-grained traceability per user story.
- Manual mention without closing keyword: rejected — does not satisfy the "linked to the PR that resolves it" requirement (closing keywords create the formal GitHub link).

---

## 10. Invitation Expiry

**Decision**: Lazy evaluation. `TeamInvitation.CreatedAt` is stored. When a Player accepts or declines, the `TeamService` checks `CreatedAt + 7 days < UtcNow`; if true, the invitation is treated as Expired and the action is rejected. No background job needed.

**Rationale**: Simple; no background service dependency. Expiry is a soft enforcement meaningful only at acceptance time.

**Alternatives Considered**:
- `IHostedService` background job: rejected — unnecessary complexity for course scope.
- No expiry: rejected — spec assumption states 7-day default.

---

## 11. Admin Account Bootstrap

**Decision**: One Admin account is seeded in `AppDbContext.OnModelCreating` using `HasData`. Email and BCrypt-hashed password are read from environment variables `ADMIN_EMAIL` and `ADMIN_PASSWORD`. The hashed value is computed once (e.g., during initial setup) and stored in configuration — or the seeding code runs at startup and applies the hash dynamically via a hosted service if the Admin row does not yet exist.

**Rationale**: Spec assumption: "At least one Admin account exists at system startup (seeded or configured via environment)." Environment variables prevent credential hardcoding (constitution "No secrets" gate).

**Alternatives Considered**:
- Hardcoded credentials in migration: rejected — violates constitution.
- Post-deploy manual script: rejected — not reproducible.

---

## 12. Angular 21 Frontend Architecture

**Decision**:
- Standalone components or feature modules (decide per Angular 21 best practices — standalone is now the Angular default).
- `CoreModule` (or core providers): Auth service, JWT interceptor (`HttpInterceptorFn`), SignalR service, route guards (`AuthGuard`, `RoleGuard`).
- Lazy-loaded feature routes for `tournaments`, `teams`, `matches`, `admin`.
- `HttpClient` for all API calls; `@microsoft/signalr` for real-time.
- `index.html` includes `lang="es"` (or `"en"`), `meta charset`, `meta viewport`, and descriptive `title`.

**Rationale**: Lazy loading satisfies Principle IV (Fast by Default). Standalone components reduce boilerplate in Angular 21.

**Alternatives Considered**:
- Server-side rendering (Angular Universal): rejected — not required; adds complexity.
- Third-party UI library (e.g., Angular Material): optional — acceptable if it reduces custom CSS complexity.
