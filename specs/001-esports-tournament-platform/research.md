# Research & Technical Decisions

**Feature**: Esports Tournament Platform  
**Date**: 2026-04-30

## Authentication Strategy

### Decision
Implement JWT (JSON Web Tokens) for stateless authentication.

### Rationale
- **Stateless**: No server-side session storage required, scales horizontally
- **Standard**: Industry-standard for SPA + API architecture
- **Cross-platform**: Works seamlessly between .NET backend and Angular frontend
- **Secure**: Tokens are signed, can include claims for roles (Organizer/Player)
- **Performance**: No database lookup on every request after token validation

### Implementation Details
- ASP.NET Core Identity for user management
- JWT token generated on login, expires after configurable time (e.g., 24 hours)
- Refresh token pattern for seamless re-authentication
- Tokens include claims: UserId, UserRole, Email
- Frontend stores token in localStorage, includes in Authorization header

### Alternatives Considered
- **Session-based auth**: Rejected - requires server-side state, doesn't scale well
- **OAuth2/OpenID Connect**: Rejected - overkill for MVP, no external identity providers needed
- **Basic Auth**: Rejected - insecure, no role-based claims

---

## Scoring System Extensibility (Strategy Pattern)

### Decision
Use Strategy Pattern with IScoringSystem interface and concrete implementations.

### Rationale
- **Open/Closed Principle**: New scoring systems added without modifying existing code
- **Academic requirement**: Demonstrates SOLID principles (FR-024)
- **Testability**: Each scoring strategy independently testable
- **Runtime selection**: Tournament stores scoring system type, resolved at runtime via DI

### Implementation Details
```csharp
public interface IScoringSystem
{
    string Name { get; }
    int CalculatePoints(MatchResult result, TeamSide side);
}

public class StandardScoringSystem : IScoringSystem
{
    public string Name => "Standard";
    public int CalculatePoints(MatchResult result, TeamSide side)
    {
        if (result.IsWinner(side)) return 3;
        if (result.IsDraw()) return 1;
        return 0;
    }
}

public class WTAScoringSystem : IScoringSystem
{
    public string Name => "WTA";
    public int CalculatePoints(MatchResult result, TeamSide side)
    {
        return result.IsWinner(side) ? 3 : 0;
    }
}

public class CustomScoringSystem : IScoringSystem
{
    private readonly int _winPoints;
    private readonly int _drawPoints;
    private readonly int _lossPoints;

    public CustomScoringSystem(int win, int draw, int loss)
    {
        _winPoints = win;
        _drawPoints = draw;
        _lossPoints = loss;
    }

    public string Name => "Custom";
    public int CalculatePoints(MatchResult result, TeamSide side)
    {
        if (result.IsWinner(side)) return _winPoints;
        if (result.IsDraw()) return _drawPoints;
        return _lossPoints;
    }
}
```

### Alternatives Considered
- **Switch/case statement**: Rejected - violates Open/Closed, requires modifying code for new systems
- **Database-driven rules engine**: Rejected - over-engineering for MVP, harder to test

---

## Database Schema Design

### Decision
Entity Framework Core Code-First with Fluent API for relationships and constraints.

### Rationale
- **Academic requirement**: Mandated by curriculum
- **Type safety**: C# entities catch errors at compile time
- **Migrations**: Automatic schema version control
- **Relationships**: Explicit navigation properties and foreign keys
- **Validation**: Combine data annotations with FluentValidation for business rules

### Key Design Patterns
- **Inheritance**: User base class with Organizer/Player derived types (TPH - Table Per Hierarchy)
- **Enums**: TournamentState, UserRole stored as integers with constraints
- **Soft deletes**: Avoid for MVP (true deletes), can add later if needed
- **Audit fields**: CreatedAt, UpdatedAt on entities requiring tracking

### Alternatives Considered
- **Database-First**: Rejected - less control, harder to version, not aligned with academic approach
- **Dapper (micro-ORM)**: Rejected - more boilerplate, loses type safety benefits

---

## Frontend State Management

### Decision
Use Angular Services with RxJS Observables for state management (no external library).

### Rationale
- **Simplicity**: No learning curve for Redux/NgRx/Akita
- **Sufficient for MVP**: App state is mostly server-driven (API responses)
- **Reactive**: RxJS built into Angular, handles async operations naturally
- **Performance**: No additional bundle size from state management libraries

### Implementation Pattern
- **Services as Stores**: Each feature module has a service managing its state
- **BehaviorSubjects**: Hold current state, emit on changes
- **Immutable updates**: Use spread operator for state updates
- **HTTP caching**: Use shareReplay for expensive queries (e.g., tournament details)

### Alternatives Considered
- **NgRx**: Rejected - overkill for MVP, steep learning curve, verbose boilerplate
- **Akita**: Rejected - additional dependency, team unfamiliar with it
- **Local Storage**: Rejected - only for auth token, not application state

---

## Error Handling Strategy

### Decision
Centralized exception handling middleware in ASP.NET Core with custom exception types.

### Rationale
- **Consistency**: All errors follow same response format
- **Security**: Never expose stack traces or internal details (500 errors)
- **Clarity**: Business errors return descriptive messages in Spanish
- **HTTP standards**: Correct status codes for different error types

### Implementation Details
```csharp
public class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (BusinessRuleException ex)
        {
            await HandleBusinessRuleException(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedException(context, ex);
        }
    }
}
```

### Custom Exception Types
- **ValidationException**: 400 - Invalid input (e.g., email format, password strength)
- **BusinessRuleException**: 400/409 - Business rule violation (e.g., tournament already started)
- **NotFoundException**: 404 - Resource not found
- **UnauthorizedException**: 401 - Not authenticated
- **ForbiddenException**: 403 - Authenticated but lacking permissions

### Alternatives Considered
- **Try-catch in every controller**: Rejected - duplicates code, inconsistent handling
- **Problem Details (RFC 7807)**: Considered for future - good standard but overkill for MVP

---

## Testing Strategy

### Decision
Mandatory unit tests with MSTest + Moq for critical features, manual test cases for end-to-end scenarios.

### Rationale
- **Academic requirement**: Unit tests required for features marked with (*)
- **Fast feedback**: Unit tests run in milliseconds, catch regressions early
- **Isolation**: Moq allows testing business logic without database/HTTP
- **Coverage**: Focus on business logic (services) and strategies (scoring systems)

### Test Organization
```
EsportsPlatform.Tests/
├── Services/
│   ├── TournamentServiceTests.cs        # Tournament creation (FR-009)
│   ├── MatchServiceTests.cs             # Match results (FR-018)
│   ├── PlayerServiceTests.cs            # Player registration (FR-026)
│   └── EnrollmentServiceTests.cs        # Team enrollment (FR-036)
├── ScoringStrategies/
│   ├── StandardScoringTests.cs
│   ├── WTAScoringTests.cs
│   └── CustomScoringTests.cs
└── Validators/
    ├── TournamentValidatorTests.cs
    └── TeamEnrollmentValidatorTests.cs
```

### Naming Convention
```csharp
[TestMethod]
public void CreateTournament_WithPastStartDate_ThrowsValidationException()
{
    // Arrange, Act, Assert
}
```

### Manual Test Cases
- Documented in `docs/documento-3-testing.md`
- Format: Preconditions, Steps, Expected Result, Actual Result
- Covers happy paths, edge cases, error scenarios

### Alternatives Considered
- **TDD (Test-Driven Development)**: Optional - team can use if comfortable, not mandatory
- **Integration tests**: Deferred to future - focus on unit tests for MVP
- **E2E tests (Selenium/Cypress)**: Out of scope - manual testing sufficient for academic project

---

## API Versioning

### Decision
No versioning for MVP (v1 implicit).

### Rationale
- **YAGNI**: No breaking changes expected during development
- **Academic project**: Single iteration, no backward compatibility needs
- **Future-ready**: Can add URL versioning (/api/v2/) or header-based if needed later

### Alternatives Considered
- **URL versioning**: /api/v1/tournaments - can add when needed
- **Header versioning**: Accept: application/vnd.esports.v1+json - over-engineering for MVP
- **Query param versioning**: /api/tournaments?version=1 - not RESTful

---

## Deployment Strategy (Out of Scope for MVP)

### Decision
Defer deployment planning to post-MVP.

### Rationale
- **Academic focus**: Project evaluated on code quality, not deployment
- **Local development**: Team runs backend (dotnet run) and frontend (ng serve) locally
- **Video demo**: Sufficient to show all functionality running on localhost

### Future Considerations
- Backend: Docker container → Azure App Service or AWS Elastic Beanstalk
- Frontend: ng build → Azure Static Web Apps or Netlify
- Database: Azure SQL Database or AWS RDS SQL Server

---

## Summary of Key Decisions

| Aspect | Decision | Primary Benefit |
|--------|----------|-----------------|
| Auth | JWT tokens | Stateless scalability |
| Scoring | Strategy Pattern | Open/Closed compliance |
| Database | EF Core Code-First | Type safety + migrations |
| State (Frontend) | Services + RxJS | Simplicity, no extra deps |
| Error Handling | Middleware + custom exceptions | Consistency + security |
| Testing | MSTest + Moq for units | Fast + isolated |
| API Versioning | None (implicit v1) | YAGNI for MVP |
| Deployment | Deferred | Focus on code quality |

**All research complete. No NEEDS CLARIFICATION remaining. Ready for Phase 1 (data-model.md).**
