# Esports Tournament Platform

Full-stack web application for managing esports tournaments with automatic standings calculation.

**University Project**: Ingeniería de Software con IA - Obligatorio 1

## Overview

Platform enabling:
- **Organizers**: Create and manage tournaments
- **Players**: Form teams and enroll in tournaments
- **System**: Automatic standings calculation with pluggable scoring systems

## Technology Stack

### Backend
- **.NET 10** - Web API
- **Entity Framework Core 10** - ORM with Code-First
- **SQL Server** - Database
- **MSTest + Moq** - Unit testing

### Frontend
- **Angular 21** - SPA framework
- **TypeScript** - Strict mode
- **RxJS** - Reactive programming

### Development Tools
- **GitHub Copilot** - AI coding assistant
- **Spec Kit** - Spec-Driven Development
- **Custom Agent Skills** - Code validation and generation

## Project Structure

```
iaModerna/
├── backend/              # .NET 10 Web API
│   ├── src/
│   │   ├── Project.Api/          # Controllers, middleware
│   │   ├── Project.Domain/       # Entities, interfaces
│   │   ├── Project.Application/  # Business logic, services
│   │   └── Project.Infrastructure/ # EF Core, repositories
│   └── tests/
│       ├── Project.UnitTests/        # MSTest unit tests
│       └── Project.IntegrationTests/ # API integration tests
│
├── frontend/             # Angular 21 SPA
│   ├── src/              # Application source
│   └── tests/            # Jasmine/Karma tests
│
├── infra/                # Infrastructure
│   └── db/               # Database migrations and scripts
│
├── docs/                 # Documentation
│   ├── agent-skills/     # Agent Skills documentation
│   ├── design/           # Design documents
│   └── api/              # API documentation
│
├── specs/                # Feature specifications
│   └── 001-esports-tournament-platform/
│       ├── spec.md       # Feature specification
│       ├── plan.md       # Implementation plan
│       ├── tasks.md      # Implementation tasks
│       ├── data-model.md # Database schema
│       └── contracts/    # API contracts
│
└── .github/
    ├── agents/           # Custom Agent Skills
    ├── prompts/          # Spec Kit prompts
    └── copilot-instructions.md  # Copilot context
```

## Quick Start

### Prerequisites
- .NET 10 SDK
- Node.js 20+ with npm
- SQL Server (local or Docker)
- Angular CLI 21

### Backend Setup
```powershell
cd backend

# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update --project src/Project.Infrastructure --startup-project src/Project.Api

# Run API (http://localhost:5000)
dotnet run --project src/Project.Api

# Run tests
dotnet test
```

### Frontend Setup
```powershell
cd frontend

# Install dependencies
npm install

# Run development server (http://localhost:4200)
ng serve

# Run tests
ng test

# Build for production
ng build --configuration production
```

### Database Setup (Docker)
```powershell
# Run SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourPassword123!" `
  -p 1433:1433 --name sqlserver-local `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

## Architecture

### Clean Architecture
```
API → Application → Domain ← Infrastructure
```

**Principles**:
- Controllers handle HTTP only
- Services contain business logic
- Repositories handle data access only
- Unidirectional dependencies

### SOLID Principles
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Scoring systems extensible via Strategy Pattern
- **Dependency Inversion**: Depend on abstractions (interfaces)

### Strategy Pattern (Scoring Systems)
Extensible scoring without modifying existing code:
- `StandardScoringStrategy` (3-1-0)
- `WtaScoringStrategy` (3-0-0)
- `CustomScoringStrategy` (configurable)

## Development Workflow

### Using Spec Kit

#### Manual Implementation (with approval)
```bash
# Implement specific task with step-by-step approval
speckit implement T001
```

#### Automatic Implementation (single task)
```
# Backend task with automatic validation
@workspace /backend-implement T002

# Frontend task
@workspace /frontend-implement T003
```

#### Automatic Orchestration (all tasks)
```
# Implement all tasks sequentially
@workspace /orchestrator

# Implement specific phase
@workspace /orchestrator phase=2

# Resume from specific task
@workspace /orchestrator start-from=T005
```

### Custom Agent Skills

#### SOLID Reviewer
Validates C# code for SOLID principles and Clean Code:
```
@workspace /solid-reviewer backend/src/Project.Application/Services/TournamentService.cs
```

#### Test Generator
Generates MSTest unit tests with Moq:
```
@workspace /test-generator backend/src/Project.Application/Services/TournamentService.cs
```

## Coding Standards

### ZERO COMMENTS Policy
Code must be self-documenting:
- Descriptive method and variable names
- Small, focused functions (<20 lines)
- Extract methods instead of comment blocks

### Clean Code
- No magic numbers (use named constants)
- Meaningful names in English
- Single responsibility per method
- Early returns (avoid nested ifs)

### Testing
Unit tests required for:
- Tournament creation (*)
- Match result registration (*)
- Player account registration (*)
- Tournament enrollment (*)

Test naming: `[MethodName]_[Scenario]_[ExpectedResult]`

## API Documentation

Swagger UI available at: `http://localhost:5000/swagger` (development)

### Key Endpoints
- `GET /api/tournaments` - List tournaments
- `POST /api/tournaments` - Create tournament (Organizer)
- `POST /api/teams` - Create team (Player)
- `POST /api/matches/{id}/result` - Register match result (Organizer)
- `GET /api/tournaments/{id}/standings` - Get standings

### Status Codes
- **200 OK** - Successful GET/PUT
- **201 Created** - Successful POST
- **400 Bad Request** - Validation errors
- **401 Unauthorized** - Authentication required
- **403 Forbidden** - Insufficient permissions
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server errors

## Documentation

- [Feature Specification](specs/001-esports-tournament-platform/spec.md)
- [Implementation Plan](specs/001-esports-tournament-platform/plan.md)
- [Data Model](specs/001-esports-tournament-platform/data-model.md)
- [API Contracts](specs/001-esports-tournament-platform/contracts/api-endpoints.md)
- [Tasks](specs/001-esports-tournament-platform/tasks.md)
- [Constitution](/.specify/memory/constitution.md)
- [Agent Skills Guide](docs/agent-skills/architecture-guide.md)

## Contributing

### Branch Strategy
- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - Feature branches

### Commit Message Format
```
<type>(<scope>): <subject>

<body>
```

Types: feat, fix, docs, refactor, test, chore

Example:
```
feat(tournaments): add tournament creation endpoint

Implement POST /api/tournaments with validation for start/end dates and max teams.

Closes #12
```

## Team

- **Course**: Ingeniería de Software con IA
- **Assignment**: Obligatorio 1
- **Due Date**: May 12, 2026

## License

Academic project - Universidad ORT Uruguay