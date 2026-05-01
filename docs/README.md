# Documentation

Project documentation, design decisions, and development guides

## Structure

```
docs/
├── agent-skills/              # Agent Skills documentation
│   ├── architecture-guide.md  # Complete agent system guide
│   └── solid-reviewer-test-cases.md # Test cases for obligatorio
├── design/                    # Design documents
├── api/                       # API documentation
└── README.md                  # This file
```

## Quick Links

### Specification & Planning
- [Feature Specification](../specs/001-esports-tournament-platform/spec.md)
- [Implementation Plan](../specs/001-esports-tournament-platform/plan.md)
- [Data Model](../specs/001-esports-tournament-platform/data-model.md)
- [API Contracts](../specs/001-esports-tournament-platform/contracts/api-endpoints.md)
- [Tasks](../specs/001-esports-tournament-platform/tasks.md)

### Project Standards
- [Constitution](../.specify/memory/constitution.md) - Core principles and coding standards

### Agent Skills
- [Architecture Guide](agent-skills/architecture-guide.md) - Complete agent system documentation
- [SOLID Reviewer Test Cases](agent-skills/solid-reviewer-test-cases.md) - Obligatorio documentation

## Architecture Overview

### Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│         Project.Api (Controllers)       │  ← HTTP Layer
├─────────────────────────────────────────┤
│    Project.Application (Services)       │  ← Business Logic
├─────────────────────────────────────────┤
│      Project.Domain (Entities)          │  ← Domain Models
├─────────────────────────────────────────┤
│  Project.Infrastructure (Repositories)  │  ← Data Access
└─────────────────────────────────────────┘
```

**Dependency Flow**: API → Application → Domain ← Infrastructure

### Key Patterns

#### Strategy Pattern (Scoring Systems)
Extensible scoring without modifying existing code:
- `IScoringStrategy` interface
- `StandardScoringStrategy` (3-1-0)
- `WtaScoringStrategy` (3-0-0)
- `CustomScoringStrategy` (configurable)

#### Repository Pattern
Abstraction over data access:
- `ITournamentRepository` interface
- `TournamentRepository` implementation (EF Core)

#### Service Layer
Business logic separated from infrastructure:
- `TournamentService` - Tournament operations
- `MatchService` - Match result registration
- `StandingsService` - Standings calculation

## Development Workflow

### 1. Feature Development
```bash
# Create feature branch
git checkout -b feature/user-story-1

# Implement feature following Clean Architecture
# - Domain entities first
# - Repository interfaces
# - Service implementations
# - Controller endpoints

# Write unit tests (MSTest + Moq)
dotnet test

# Commit with conventional commits
git commit -m "feat: implement tournament creation endpoint"
```

### 2. Code Review
- Verify SOLID principles
- Check ZERO COMMENTS compliance
- Run SOLID reviewer agent: `@workspace /solid-reviewer <file>`
- Ensure tests pass
- Verify API follows REST conventions

### 3. Integration
```bash
# Merge to develop
git checkout develop
git merge feature/user-story-1

# Push to remote
git push origin develop
```

## Testing Strategy

### Unit Tests (Required for *)
Features requiring unit tests:
1. Tournament creation (*)
2. Match result registration (*)
3. Player account registration (*)
4. Tournament enrollment (*)

### Test Coverage Goals
- Services: 80%+ coverage
- Critical paths: 100% coverage
- Edge cases: Document in test cases

### Manual Test Cases
Document in `docs/test-cases/`:
- Preconditions
- Steps to execute
- Expected result
- Actual result

## API Documentation

### Swagger/OpenAPI
Available at: `http://localhost:5000/swagger` (development)

### Key Endpoints

#### Tournaments
- `GET /api/tournaments` - List all tournaments
- `GET /api/tournaments/{id}` - Get tournament details
- `POST /api/tournaments` - Create tournament (Organizer only)
- `PUT /api/tournaments/{id}` - Update tournament (Organizer only)
- `DELETE /api/tournaments/{id}` - Delete tournament (Organizer only)

#### Teams
- `GET /api/teams` - List all teams
- `POST /api/teams` - Create team (Player only)
- `POST /api/teams/{id}/invite` - Invite player to team
- `POST /api/teams/{id}/accept` - Accept team invitation

#### Matches
- `GET /api/tournaments/{id}/matches` - Get tournament matches
- `POST /api/matches/{id}/result` - Register match result (Organizer only)

#### Standings
- `GET /api/tournaments/{id}/standings` - Get tournament standings

## Troubleshooting

### Common Issues

#### SOLID Violations
Run solid-reviewer agent to identify issues:
```
@workspace /solid-reviewer backend/src/Project.Application/Services/TournamentService.cs
```

Common violations:
- SRP: Service doing validation + persistence (extract validator)
- Magic numbers: Hardcoded values (extract constants)
- Long methods: >20 lines (extract sub-methods)

#### Test Failures
```powershell
# Run specific test
dotnet test --filter "FullyQualifiedName~TournamentServiceTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

#### EF Core Issues
```powershell
# Reset database
dotnet ef database drop --project backend/src/Project.Infrastructure --startup-project backend/src/Project.Api
dotnet ef database update --project backend/src/Project.Infrastructure --startup-project backend/src/Project.Api
```

## Contributing

### Code Standards
Follow [Constitution](../.specify/memory/constitution.md):
- ZERO COMMENTS policy
- Clean Architecture
- SOLID principles (minimum 2 documented)
- Small methods (<20 lines)
- Meaningful names

### Commit Message Format
```
<type>(<scope>): <subject>

<body>

<footer>
```

Types: feat, fix, docs, refactor, test, chore

Example:
```
feat(tournaments): add tournament creation endpoint

Implement POST /api/tournaments with validation:
- Start date must be in future
- End date must be after start date
- Max teams >= 2

Closes #12
```

## Resources

### External Links
- [.NET 10 Documentation](https://learn.microsoft.com/dotnet/)
- [Angular 21 Documentation](https://angular.io/docs)
- [EF Core Documentation](https://learn.microsoft.com/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

### Internal Resources
- [Spec Kit Documentation](../.specify/README.md)
- [Agent Skills Guide](agent-skills/architecture-guide.md)
