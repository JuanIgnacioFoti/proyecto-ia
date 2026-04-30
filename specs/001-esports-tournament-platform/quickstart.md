# Quick Start Guide

**Feature**: Esports Tournament Platform  
**Last Updated**: 2026-04-30

## Prerequisites

- .NET 10 SDK
- Node.js 18+ (for Angular 21)
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 / VS Code / Rider
- Git

## Initial Setup

### 1. Clone Repository

```bash
git clone https://github.com/JuanIgnacioFoti/proyecto-ia.git
cd proyecto-ia
git checkout develop
```

### 2. Backend Setup

```bash
cd backend
dotnet restore
dotnet ef database update --project EsportsPlatform.API
dotnet run --project EsportsPlatform.API
```

Backend runs on `https://localhost:5001` (or configured port).

### 3. Frontend Setup

```bash
cd frontend
npm install
ng serve
```

Frontend runs on `http://localhost:4200`.

### 4. Run Tests

```bash
cd backend/EsportsPlatform.Tests
dotnet test
```

## Project Structure

```
backend/
├── EsportsPlatform.API/           # ASP.NET Core Web API
├── EsportsPlatform.Application/   # Services (business logic)
├── EsportsPlatform.Domain/        # Entities + interfaces
├── EsportsPlatform.Infrastructure/ # EF Core + repositories
└── EsportsPlatform.Tests/         # MSTest unit tests

frontend/
└── src/app/
    ├── features/                   # Feature modules
    ├── core/                       # Singletons (auth, interceptors)
    └── shared/                     # Reusable components

specs/001-esports-tournament-platform/
├── spec.md                         # Requirements
├── plan.md                         # This implementation plan
├── research.md                     # Technical decisions
├── data-model.md                   # Database schema
└── contracts/                      # API contracts
```

## Development Workflow

### 1. Create Feature Branch

```bash
git checkout develop
git pull origin develop
git checkout -b feature/tournament-creation
```

### 2. Implement Feature

Follow Clean Architecture:
1. Define entities in `Domain/`
2. Create repository interface in `Domain/Interfaces/`
3. Implement repository in `Infrastructure/`
4. Create service in `Application/Services/`
5. Add controller in `API/Controllers/`
6. Write unit tests in `Tests/`

### 3. Run Locally

Terminal 1 (Backend):
```bash
cd backend/EsportsPlatform.API
dotnet watch run
```

Terminal 2 (Frontend):
```bash
cd frontend
ng serve
```

Terminal 3 (Tests):
```bash
cd backend/EsportsPlatform.Tests
dotnet watch test
```

### 4. Commit & Push

```bash
git add .
git commit -m "feat(tournaments): add tournament creation endpoint"
git push origin feature/tournament-creation
```

### 5. Create Pull Request

Open PR from feature branch to `develop` on GitHub.

## Testing

### Unit Tests (Backend)

```bash
cd backend/EsportsPlatform.Tests
dotnet test --logger "console;verbosity=detailed"
```

Required coverage:
- Tournament creation (TournamentServiceTests)
- Match results (MatchServiceTests)
- Player registration (PlayerServiceTests)
- Team enrollment (EnrollmentServiceTests)

### Manual Test Cases

Documented in `docs/documento-3-testing.md`. Execute locally:
1. Register organizer → Create tournament → Enroll teams
2. Register players → Form team → Enroll in tournament
3. Register match results → Verify standings

## Common Commands

### Database Migrations

Create migration:
```bash
cd backend
dotnet ef migrations add MigrationName --project EsportsPlatform.Infrastructure --startup-project EsportsPlatform.API
```

Apply migration:
```bash
dotnet ef database update --project EsportsPlatform.Infrastructure --startup-project EsportsPlatform.API
```

Reset database:
```bash
dotnet ef database drop --project EsportsPlatform.Infrastructure --startup-project EsportsPlatform.API
dotnet ef database update --project EsportsPlatform.Infrastructure --startup-project EsportsPlatform.API
```

### Angular Commands

Generate component:
```bash
cd frontend
ng generate component features/tournaments/tournament-list
```

Generate service:
```bash
ng generate service features/tournaments/tournament
```

Build for production:
```bash
ng build --configuration production
```

## Troubleshooting

### Backend won't start
- Check SQL Server is running
- Verify connection string in `appsettings.json`
- Run `dotnet ef database update`

### Frontend won't compile
- Delete `node_modules/` and run `npm install`
- Check Node version: `node --version` (should be 18+)
- Clear Angular cache: `ng cache clean`

### Tests failing
- Check database is in known state (rerun migrations)
- Verify mocks are configured correctly
- Run single test: `dotnet test --filter FullyQualifiedName~TestMethodName`

## Key Files

- **Constitution**: `.specify/memory/constitution.md`
- **Spec**: `specs/001-esports-tournament-platform/spec.md`
- **API Contracts**: `specs/001-esports-tournament-platform/contracts/api-endpoints.md`
- **Data Model**: `specs/001-esports-tournament-platform/data-model.md`
- **Copilot Instructions**: `.github/copilot-instructions.md`

## Next Steps

1. Review specification: `specs/001-esports-tournament-platform/spec.md`
2. Understand data model: `specs/001-esports-tournament-platform/data-model.md`
3. Implement P1 user stories first (organizer registration, tournament creation)
4. Use `/speckit.tasks` to break down work
5. Use `/speckit.implement` to execute tasks

**Ready to code!** 🚀
