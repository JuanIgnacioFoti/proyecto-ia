# Implementation Plan: Esports Tournament Platform

**Branch**: `develop` | **Date**: 2026-04-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-esports-tournament-platform/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Build a web-based esports tournament management platform enabling organizers to create and manage tournaments, players to form teams and enroll, and automatic standings calculation using pluggable scoring systems. The platform supports multiple videogames, role-based access control, tournament state machine, and real-time standings updates.

**Technical Approach**: Clean Architecture with .NET 10 backend (API + EF Core), Angular 21 SPA frontend, SQL Server database, Strategy Pattern for extensible scoring systems, and comprehensive unit testing with MSTest.

## Technical Context

**Language/Version**: 
- Backend: C# / .NET 10
- Frontend: TypeScript / Angular 21

**Primary Dependencies**: 
- Backend: ASP.NET Core Web API, Entity Framework Core 10, SQL Server Driver
- Frontend: Angular 21, RxJS, Angular Router, HttpClient
- Testing: MSTest, Moq (backend), Jasmine/Karma (frontend)

**Storage**: SQL Server (Code-First EF Core migrations)

**Testing**: 
- Backend: MSTest + Moq for unit tests (required for features with *)
- Frontend: Jasmine + Karma for unit tests
- Manual test cases documented for critical features

**Target Platform**: 
- Backend: Cross-platform (.NET 10 - Windows/Linux/macOS)
- Frontend: Modern browsers (Chrome, Firefox, Edge, Safari - latest versions)
- Deployment: Web server (IIS/Kestrel) + static file hosting

**Project Type**: Full-stack web application (SPA + RESTful API)

**Performance Goals**: 
- API response time: <500ms p95 for queries, <1s for complex operations
- Standings calculation: <2 seconds after match result registration
- Support 100 concurrent organizers creating tournaments
- Support 1,000 concurrent players viewing standings

**Constraints**: 
- Authentication mechanism: Team's choice (JWT recommended for stateless API)
- Error responses: 400 (validation), 401 (unauthenticated), 403 (unauthorized), 404 (not found), 500 (server error without internal details)
- Clean Architecture: Controllers → Services → Repositories (unidirectional dependencies)
- ZERO comments policy: Code must be self-documenting through naming
- Strategy Pattern mandatory for scoring systems (Open/Closed Principle)
- Unit tests mandatory for: tournament creation, match results, player registration, team enrollment

**Scale/Scope**: 
- MVP: Up to 1,000 concurrent users
- Estimated: 50+ API endpoints, 20+ Angular components
- Database: ~10-15 tables
- Features: 43 functional requirements across 3 user roles

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Clean Architecture (NON-NEGOTIABLE)
- [x] Separation of responsibilities: Controllers (HTTP), Services (business logic), Repositories (data access)
- [x] Unidirectional dependencies: Business logic → Abstractions (interfaces), NOT infrastructure
- [x] Testability: All business logic injectable and mockable

### ✅ SOLID Principles (Minimum 2 Documented)
- [x] **Single Responsibility**: Each service handles one domain aggregate
- [x] **Open/Closed**: Scoring system extensible via Strategy Pattern (IScoringSystem interface)
- [x] **Liskov Substitution**: All scoring implementations interchangeable
- [x] **Interface Segregation**: Specific interfaces per domain (ITeamRepository, ITournamentRepository, IScoringSystem)
- [x] **Dependency Inversion**: Services depend on repository interfaces, not concrete implementations

**Documentation Plan**: Document in Documento 1 with specific class references (e.g., StandardScoringStrategy, WTAScoringStrategy, CustomScoringStrategy implementing IScoringSystem)

### ✅ Clean Code (Always)
- [x] English names for all code elements
- [x] Expressive names revealing intent (no QUÉ comments needed)
- [x] Small functions with single responsibility
- [x] No commented code
- [x] No magic numbers (use constants/enums)
- [x] No debug prints in production
- [x] **ZERO Comments**: Code must be self-documenting through naming and structure

### ✅ RESTful API Standards
- [x] Resources as plural nouns: `/api/tournaments`, `/api/teams`, `/api/players`, `/api/matches`
- [x] Correct HTTP verbs: GET, POST, PUT/PATCH, DELETE
- [x] Appropriate status codes: 200, 201, 204, 400, 401, 403, 404, 500
- [x] Consistent JSON response structure (camelCase frontend, PascalCase backend)

### ✅ Testing (NON-NEGOTIABLE for features with *)
- [x] Unit tests with MSTest + Moq for:
  - Tournament creation (FR-009)
  - Match results registration (FR-018)
  - Player registration (FR-026)
  - Team tournament enrollment (FR-036)
- [x] Descriptive test names (method_scenario_expectedResult pattern)
- [x] Manual test cases documented with: preconditions, steps, expected result, actual result

### ✅ Centralized Error Handling (NON-NEGOTIABLE)
- [x] Exception handling middleware planned
- [x] Validation errors return 400 with descriptive messages (Spanish)
- [x] Business errors return appropriate codes (400/404/409)
- [x] Authentication returns 401, authorization returns 403
- [x] Unhandled exceptions return 500 without exposing internals

### ⚠️ Complexity Justification Required

**Strategy Pattern for Scoring Systems**: Justified - Required by FR-024 (extensibility without code modification) and academic requirement to demonstrate Open/Closed Principle.

**No violations** - All complexity aligns with constitution and academic requirements.

## Project Structure

### Documentation (this feature)

```text
specs/001-esports-tournament-platform/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── api-endpoints.md
│   └── request-response-schemas.md
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── EsportsPlatform.API/              # ASP.NET Core Web API project
│   ├── Controllers/                   # HTTP controllers (no business logic)
│   ├── Middleware/                    # Exception handling, auth
│   ├── Program.cs
│   └── appsettings.json
├── EsportsPlatform.Application/      # Business logic layer
│   ├── Services/                      # Service classes (ITournamentService, etc.)
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Interfaces/                    # Service interfaces
│   └── Validators/                    # FluentValidation or custom validators
├── EsportsPlatform.Domain/           # Domain entities and business rules
│   ├── Entities/                      # Tournament, Team, Player, Match, etc.
│   ├── Enums/                         # TournamentState, UserRole
│   ├── Interfaces/                    # Repository interfaces
│   └── ScoringStrategies/             # IScoringSystem + implementations
├── EsportsPlatform.Infrastructure/   # Data access and external concerns
│   ├── Data/                          # EF Core DbContext
│   ├── Repositories/                  # Repository implementations
│   └── Migrations/                    # EF Core migrations
└── EsportsPlatform.Tests/            # MSTest unit tests
    ├── Services/                      # Service tests with Moq
    ├── Validators/                    # Validation tests
    └── ScoringStrategies/             # Scoring system tests

frontend/
├── src/
│   ├── app/
│   │   ├── core/                      # Singletons (auth, http interceptors)
│   │   ├── shared/                    # Shared components, directives, pipes
│   │   ├── features/                  # Feature modules
│   │   │   ├── tournaments/           # Tournament management
│   │   │   ├── teams/                 # Team management
│   │   │   ├── players/               # Player registration/profile
│   │   │   └── auth/                  # Authentication
│   │   ├── models/                    # TypeScript interfaces
│   │   └── services/                  # API client services
│   ├── assets/
│   └── environments/
└── tests/                             # Jasmine/Karma tests

docs/                                  # Academic deliverables
├── documento-1-diseno.md             # Design & technical decisions
├── documento-2-ia-proceso.md         # AI prompts diary
└── documento-3-testing.md            # Test reports & manual test cases
```

**Structure Decision**: Full-stack web application with clear backend/frontend separation. Backend follows Clean Architecture (4 projects: API, Application, Domain, Infrastructure) enforcing unidirectional dependencies. Frontend uses Angular feature modules pattern for scalability. Tests colocated with source code.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | No violations | All design decisions align with constitution |

**Note**: Strategy Pattern for scoring systems is not a violation - it's explicitly required by FR-024 and constitution's Open/Closed Principle mandate.
