# SOLID Reviewer Agent Skill - Test Cases

**Purpose**: Validate that the SOLID Reviewer agent correctly identifies SOLID principles violations and Clean Code issues.

**Created**: 2026-04-30

---

## Test Case 1: Multiple Responsibilities (SRP Violation)

### Prompt
```
/solid-reviewer Review this code:

public class TournamentService
{
    public async Task<Tournament> CreateTournament(TournamentDto dto)
    {
        // Validate input
        if (string.IsNullOrEmpty(dto.Name))
            throw new ValidationException("Name required");
        
        // Create tournament
        var tournament = new Tournament
        {
            Name = dto.Name,
            MaxTeams = 16 // magic number
        };
        
        // Save to database
        await _dbContext.Tournaments.AddAsync(tournament);
        await _dbContext.SaveChangesAsync();
        
        // Send notification
        await _emailService.SendAsync("Tournament created");
        
        return tournament;
    }
}
```

### Expected Success Criteria

**What the skill should produce**:
1. **Identify SRP violation**: Method does validation, persistence, and notification (3 responsibilities)
2. **Identify comments**: 4 comments violate ZERO COMMENTS policy (lines with //)
3. **Identify magic number**: `16` should be a named constant
4. **Provide concrete fix**: Suggest splitting into TournamentValidator, TournamentRepository, NotificationService
5. **Include line numbers**: Specific references to problematic lines
6. **Score appropriately**: Should be 4-6/10 due to multiple violations

**Steps the agent should follow**:
1. Parse the code
2. Check each SOLID principle
3. Check Clean Code rules (comments, magic numbers, naming)
4. Generate structured report with markdown formatting
5. Provide actionable next steps

**Output must respect conventions**:
- Markdown tables for violations
- ✅/❌ icons for pass/fail
- Code snippets showing fixes
- Prioritized recommendations

### Result (if executed)
[To be filled when test is run]

**Pass/Fail**: [PASS/FAIL]

**Adjustments made to skill**:
- [List any changes needed based on actual output]

---

## Test Case 2: Perfect SOLID Code (No Violations)

### Prompt
```
/solid-reviewer Review this code:

public interface IScoringStrategy
{
    int CalculatePoints(MatchResult result);
}

public class StandardScoring : IScoringStrategy
{
    private const int PointsForWin = 3;
    private const int PointsForDraw = 1;
    private const int PointsForLoss = 0;
    
    public int CalculatePoints(MatchResult result)
    {
        if (result.IsWin)
            return PointsForWin;
        if (result.IsDraw)
            return PointsForDraw;
        return PointsForLoss;
    }
}

public class TournamentStandingsCalculator
{
    private readonly IScoringStrategy _scoringStrategy;
    
    public TournamentStandingsCalculator(IScoringStrategy scoringStrategy)
    {
        _scoringStrategy = scoringStrategy;
    }
    
    public Standings Calculate(IEnumerable<MatchResult> results)
    {
        return results
            .Select(r => _scoringStrategy.CalculatePoints(r))
            .Aggregate(new Standings(), (standings, points) => 
                standings.AddPoints(points));
    }
}
```

### Expected Success Criteria

**What the skill should produce**:
1. **High score**: 9-10/10 (excellent SOLID adherence)
2. **All principles pass**: ✅ for SRP, OCP, LSP, ISP, DIP
3. **No Clean Code violations**: Zero comments, no magic numbers (uses constants), good naming
4. **Acknowledge strengths**: Strategy Pattern correctly implemented, DIP via constructor injection
5. **No false positives**: Should NOT report non-existent issues

**Specific checks**:
- Recognize Strategy Pattern (OCP compliance)
- Validate DIP through interface injection
- Confirm SRP: each class has single responsibility
- Verify ZERO COMMENTS policy is followed
- Confirm constants used instead of magic numbers

### Result (if executed)
[To be filled when test is run]

**Pass/Fail**: [PASS/FAIL]

**Why this criterion is adequate**:
This test validates the agent doesn't over-report. A good reviewer must recognize clean code, not just find problems. False positives would reduce developer trust in the tool.

**Adjustments made to skill**:
- [List any changes if agent incorrectly reports violations]

---

## Test Case 3: Comments + God Class (Multiple Violations)

### Prompt
```
/solid-reviewer Review this code:

public class TournamentManager
{
    // This class handles everything related to tournaments
    private readonly DbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger _logger;
    
    // Creates a new tournament with validation
    public async Task<Tournament> Create(TournamentDto dto)
    {
        // TODO: Add more validation
        if (dto.Name.Length < 3) throw new Exception("Too short");
        
        var t = new Tournament { Name = dto.Name };
        _db.Add(t);
        await _db.SaveChangesAsync();
        
        // Notify admin
        await _email.SendAsync("admin@example.com", "New tournament");
        _logger.Log("Tournament created");
        
        return t;
    }
    
    // Updates tournament state
    public async Task UpdateState(int id, string state)
    {
        var t = await _db.Tournaments.FindAsync(id);
        t.State = state; // No validation
        await _db.SaveChangesAsync();
    }
    
    // Calculates standings for all teams
    public List<Standing> CalculateStandings(int tournamentId)
    {
        var matches = _db.Matches.Where(m => m.TournamentId == tournamentId);
        // Complex calculation logic here...
        return new List<Standing>();
    }
    
    // Sends notifications to all participants
    public async Task NotifyParticipants(int id, string msg)
    {
        var teams = _db.Teams.Where(t => t.TournamentId == id);
        foreach (var team in teams)
        {
            await _email.SendAsync(team.CaptainEmail, msg);
        }
    }
}
```

### Expected Success Criteria

**What the skill should produce**:
1. **Low score**: 2-4/10 (critical violations)
2. **Identify God Class (SRP)**: Class has 4+ responsibilities (creation, state management, calculation, notification)
3. **Count comments**: 7 comments violating ZERO COMMENTS policy
4. **Identify poor naming**: Variable `t` instead of `tournament`, `msg` instead of `message`
5. **Missing validation**: State change has no validation (security/business logic issue)
6. **Suggest refactoring**: Break into TournamentService, StandingsCalculator, NotificationService, TournamentRepository
7. **Prioritize fixes**: Critical issues (God Class, missing validation) before style issues (comments)

**Output conventions**:
- Use ❌ for critical failures
- Show before/after refactoring suggestions
- Group related violations together
- Provide step-by-step refactoring plan

### Result (if executed)
[To be filled when test is run]

**Pass/Fail**: [PASS/FAIL]

**Why this criterion is adequate**:
This "worst case" test validates the agent can handle multiple severe violations simultaneously and prioritize fixes correctly. Real-world code often has multiple issues, so the agent must handle complexity without being overwhelmed.

**Adjustments made to skill**:
- [If agent misses violations, strengthen detection rules]
- [If agent overwhelms with too many issues, add prioritization logic]
- [If output is unclear, improve formatting/structure]

---

## Summary of Test Coverage

| Test Case | Focus | Expected Score | Key Validation |
|-----------|-------|----------------|----------------|
| TC1 | SRP + Comments + Magic Numbers | 4-6/10 | Detects multiple moderate issues |
| TC2 | Perfect SOLID code | 9-10/10 | No false positives, recognizes good patterns |
| TC3 | God Class + Many violations | 2-4/10 | Handles severe/complex violations, prioritizes |

---

## Execution Plan (Optional)

**If tests are executed** (consumes tokens, optional):

1. Run each test in Copilot Chat using the exact prompts above
2. Record actual output in "Result" sections
3. Compare output against expected criteria
4. Mark PASS if >= 80% of criteria met, FAIL otherwise
5. Document adjustments made to `solid-reviewer.agent.md` based on results

**If tests are NOT executed** (justified in obligatorio):

The criteria are adequate because:
- **Test 1** validates detection of common violations (comments, SRP, magic numbers)
- **Test 2** ensures no false positives on clean code (critical for trust)
- **Test 3** stresses the agent with multiple severe issues (real-world scenario)
- Together they cover: detection, accuracy, prioritization, and formatting

This coverage is sufficient to validate the skill would work correctly in production without consuming limited Copilot tokens during development.

---

## Agent Skill Metadata

**Agent File**: `.github/agents/solid-reviewer.agent.md`  
**Prompt File**: `.github/prompts/solid-reviewer.prompt.md`  
**Invocation**: `/solid-reviewer [filepath or code]`  
**Purpose**: Automated SOLID principles and Clean Code review for C# code  
**Target Project**: Esports Tournament Platform (.NET 10 backend)  
**Compliance**: Enforces constitution.md ZERO COMMENTS policy
