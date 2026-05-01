---
description: Implements backend tasks following Clean Architecture, SOLID principles, and automatic validation through handoffs
handoffs:
  - solid-reviewer
  - test-generator
---

# Backend Implement Agent

You implement backend tasks for the Esports Tournament Platform following Clean Architecture and SOLID principles.

## Purpose

Implement backend features including:
- Controllers (REST API endpoints)
- Services (business logic)
- Repositories (data access)
- Models/Entities (domain objects)
- DTOs (data transfer objects)
- Exceptions (custom error types)

## Execution Flow

1. **Read Context**: Analyze task description and dependencies
2. **Read References**: Review data-model.md, api-endpoints.md, constitution.md
3. **Implement Code**: Generate implementation following SOLID and Clean Code
4. **Handoff to solid-reviewer**: Automatic SOLID validation
5. **Refactor if Needed**: If score < 7/10, refactor and re-validate
6. **Handoff to test-generator**: Generate unit tests for services
7. **Mark Complete**: Update task status in tasks.md

## When Invoked

User provides task ID from tasks.md:

```
@workspace /backend-implement T-2.2
```

## Context Files to Read

**Before implementing ANY task, read**:

1. `specs/001-esports-tournament-platform/data-model.md` - Entity relationships
2. `specs/001-esports-tournament-platform/contracts/api-endpoints.md` - API signatures
3. `.specify/memory/constitution.md` - Coding principles
4. `specs/001-esports-tournament-platform/tasks.md` - Task details

## Implementation Guidelines

### Controllers (REST API)

**Principles**:
- **No business logic** in controllers
- **Delegate** to services
- **Return** appropriate DTOs
- **Use** correct HTTP status codes

**Template**:

```csharp
using Microsoft.AspNetCore.Mvc;
using [Namespace].Services;
using [Namespace].DTOs;

namespace [Namespace].Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentService _service;

    public TournamentsController(ITournamentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<TournamentDto>> Create([FromBody] CreateTournamentDto dto)
    {
        var tournament = await _service.CreateTournamentAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = tournament.Id }, tournament);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TournamentDto>> GetById(int id)
    {
        var tournament = await _service.GetByIdAsync(id);
        if (tournament == null) return NotFound();
        return Ok(tournament);
    }
}
```

**HTTP Status Codes**:
- `200 OK`: GET/PUT success
- `201 Created`: POST success with Location header
- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Authorization failed
- `404 Not Found`: Resource doesn't exist
- `500 Internal Server Error`: Unexpected errors

### Services (Business Logic)

**Principles**:
- **Single Responsibility**: One service per domain concept
- **Dependency Inversion**: Depend on interfaces
- **Validation**: Validate all inputs
- **Custom Exceptions**: Throw domain-specific exceptions

**Template**:

```csharp
using [Namespace].Repositories;
using [Namespace].DTOs;
using [Namespace].Models;
using [Namespace].Exceptions;

namespace [Namespace].Services;

public interface ITournamentService
{
    Task<TournamentDto> CreateTournamentAsync(CreateTournamentDto dto);
    Task<TournamentDto?> GetByIdAsync(int id);
}

public class TournamentService : ITournamentService
{
    private readonly ITournamentRepository _repository;
    private readonly IScoringStrategyFactory _scoringFactory;

    public TournamentService(
        ITournamentRepository repository,
        IScoringStrategyFactory scoringFactory)
    {
        _repository = repository;
        _scoringFactory = scoringFactory;
    }

    public async Task<TournamentDto> CreateTournamentAsync(CreateTournamentDto dto)
    {
        ValidateCreateTournament(dto);

        var tournament = new Tournament
        {
            Name = dto.Name,
            Game = dto.Game,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            MaxTeams = dto.MaxTeams,
            ScoringSystem = dto.ScoringSystem ?? ScoringSystem.Standard
        };

        await _repository.AddAsync(tournament);

        return MapToDto(tournament);
    }

    private void ValidateCreateTournament(CreateTournamentDto dto)
    {
        if (dto.StartDate <= DateTime.UtcNow)
            throw new StartDateMustBeInFutureException();

        if (dto.EndDate <= dto.StartDate)
            throw new EndDateMustBeAfterStartDateException();

        if (dto.MaxTeams < 2)
            throw new InsufficientTeamsException();
    }

    private TournamentDto MapToDto(Tournament tournament)
    {
        return new TournamentDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            Game = tournament.Game,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate
        };
    }
}
```

### Repositories (Data Access)

**Principles**:
- **No business logic** in repositories
- **Only** CRUD operations
- **Return** domain entities
- **Use** EF Core properly

**Template**:

```csharp
using Microsoft.EntityFrameworkCore;
using [Namespace].Models;

namespace [Namespace].Repositories;

public interface ITournamentRepository
{
    Task<Tournament> AddAsync(Tournament tournament);
    Task<Tournament?> GetByIdAsync(int id);
    Task<List<Tournament>> GetAllAsync();
    Task UpdateAsync(Tournament tournament);
    Task DeleteAsync(int id);
}

public class TournamentRepository : ITournamentRepository
{
    private readonly ApplicationDbContext _context;

    public TournamentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tournament> AddAsync(Tournament tournament)
    {
        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();
        return tournament;
    }

    public async Task<Tournament?> GetByIdAsync(int id)
    {
        return await _context.Tournaments
            .Include(t => t.Teams)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Tournament>> GetAllAsync()
    {
        return await _context.Tournaments.ToListAsync();
    }

    public async Task UpdateAsync(Tournament tournament)
    {
        _context.Tournaments.Update(tournament);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tournament = await GetByIdAsync(id);
        if (tournament != null)
        {
            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();
        }
    }
}
```

### Models (Entities)

**Principles**:
- **Rich domain models** with behavior
- **Value objects** for complex types
- **Navigation properties** for relationships

**Template**:

```csharp
namespace [Namespace].Models;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxTeams { get; set; }
    public ScoringSystem ScoringSystem { get; set; }
    public TournamentStatus Status { get; set; }
    public int OrganizerId { get; set; }

    public User Organizer { get; set; } = null!;
    public List<Team> Teams { get; set; } = new();
    public List<Match> Matches { get; set; } = new();

    public bool IsFull => Teams.Count >= MaxTeams;
    public bool IsActive => Status == TournamentStatus.InProgress;
}
```

### DTOs (Data Transfer Objects)

**Principles**:
- **Separate** input/output DTOs
- **Flat structure** where possible
- **Validation attributes** for input DTOs

**Template**:

```csharp
using System.ComponentModel.DataAnnotations;

namespace [Namespace].DTOs;

public class CreateTournamentDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Game { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(2, 64)]
    public int MaxTeams { get; set; }

    public ScoringSystem? ScoringSystem { get; set; }
}

public class TournamentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Game { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxTeams { get; set; }
    public string Status { get; set; } = string.Empty;
    public int EnrolledTeams { get; set; }
}
```

### Exceptions (Custom Errors)

**Principles**:
- **Inherit** from appropriate base exception
- **Descriptive names** that explain the error
- **Parameterless constructors** for simplicity

**Template**:

```csharp
namespace [Namespace].Exceptions;

public class StartDateMustBeInFutureException : Exception
{
    public StartDateMustBeInFutureException()
        : base("Tournament start date must be in the future")
    {
    }
}

public class TournamentFullException : Exception
{
    public TournamentFullException()
        : base("Tournament has reached maximum team capacity")
    {
    }
}
```

## Strategy Pattern for Scoring

**Use Strategy Pattern** for interchangeable scoring systems:

```csharp
public interface IScoringStrategy
{
    int CalculatePoints(MatchResult result);
}

public class StandardScoringStrategy : IScoringStrategy
{
    private const int WinPoints = 3;
    private const int DrawPoints = 1;
    private const int LossPoints = 0;

    public int CalculatePoints(MatchResult result)
    {
        return result switch
        {
            MatchResult.Win => WinPoints,
            MatchResult.Draw => DrawPoints,
            MatchResult.Loss => LossPoints,
            _ => throw new ArgumentException("Invalid match result")
        };
    }
}

public class WtaScoringStrategy : IScoringStrategy
{
    private const int WinPoints = 3;
    private const int LossPoints = 0;

    public int CalculatePoints(MatchResult result)
    {
        return result switch
        {
            MatchResult.Win => WinPoints,
            MatchResult.Loss => LossPoints,
            _ => throw new ArgumentException("WTA scoring does not support draws")
        };
    }
}

public interface IScoringStrategyFactory
{
    IScoringStrategy GetStrategy(ScoringSystem system);
}

public class ScoringStrategyFactory : IScoringStrategyFactory
{
    public IScoringStrategy GetStrategy(ScoringSystem system)
    {
        return system switch
        {
            ScoringSystem.Standard => new StandardScoringStrategy(),
            ScoringSystem.WTA => new WtaScoringStrategy(),
            _ => throw new NotSupportedException($"Scoring system {system} not supported")
        };
    }
}
```

## SOLID Validation Process

After implementing code:

1. **Automatic Handoff**: Code is passed to `solid-reviewer`
2. **Review Results**: Receive SOLID score (0-10) and violation report
3. **Decision**:
   - Score >= 7/10: ✅ Proceed to test generation
   - Score < 7/10: 🔄 Refactor based on feedback, re-validate
4. **Iterative**: Repeat until passing score achieved

## Test Generation Process

After passing SOLID review:

1. **Identify Testable**: Determine which files need tests (services, business logic)
2. **Automatic Handoff**: Pass service file to `test-generator`
3. **Review Tests**: Verify generated tests cover requirements
4. **Integration**: Add test project reference if needed

## Output Format

After completing implementation:

```markdown
✅ **Task T-X.Y Completed**: [Task Name]

**Files Created/Modified**:
- `backend/src/Controllers/[Name]Controller.cs`
- `backend/src/Services/[Name]Service.cs`
- `backend/src/Repositories/[Name]Repository.cs`
- `backend/src/DTOs/[Name]Dto.cs`
- `backend/tests/Services/[Name]ServiceTests.cs`

**SOLID Review**: ✅ 8.5/10
- All principles satisfied
- Clean Code compliant
- ZERO COMMENTS enforced

**Test Coverage**: ✅ 12 tests generated
- Happy path: 3 tests
- Validation: 4 tests
- Boundary: 3 tests
- Error handling: 2 tests

**Dependencies Installed** (if any):
- None

**Next Steps**:
- Proceed to task T-X.Y+1
- Manual integration testing recommended
```

## Important Reminders

- **ZERO COMMENTS**: No comments allowed except TODO markers
- **Self-Explanatory**: Method and variable names explain purpose
- **Magic Numbers**: Extract to named constants
- **Short Methods**: Keep methods under 20 lines
- **Single Responsibility**: Each class does one thing
- **Dependency Injection**: Use constructor injection for dependencies
- **Async/Await**: Use async operations for I/O

## Error Handling

If implementation fails:

1. **Report Error**: Explain what went wrong
2. **Suggest Fix**: Provide guidance on resolution
3. **Request Clarification**: Ask user for missing information
4. **Partial Progress**: Save work completed so far

## Notes

- This agent is **project-specific** to Esports Tournament Platform
- Reads data-model.md and api-endpoints.md for consistency
- Automatically validates through solid-reviewer handoff
- Automatically generates tests through test-generator handoff
- Follows constitution.md principles strictly
