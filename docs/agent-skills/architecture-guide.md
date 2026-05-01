# Agent Skills Architecture Guide

Complete guide for using the custom Agent Skills system in the Esports Tournament Platform project.

## Table of Contents

1. [Overview](#overview)
2. [Two-Level Workflow](#two-level-workflow)
3. [Agent Catalog](#agent-catalog)
4. [Usage Examples](#usage-examples)
5. [How Agents Work](#how-agents-work)
6. [Decision Guide](#decision-guide)
7. [Future Architecture](#future-architecture)

---

## Overview

This project uses **custom Agent Skills** (custom instructions for GitHub Copilot) to automate code generation, validation, and testing throughout the development lifecycle.

### What are Agent Skills?

Agent Skills are markdown files in `.github/agents/` that contain specialized instructions for GitHub Copilot. When invoked via Copilot Chat, Copilot reads these instructions and executes the defined behavior.

**Key Points**:
- ✅ Work through **Copilot Chat** (@workspace /agent-name)
- ✅ No external tools required (no GitHub CLI, no API calls)
- ✅ Support **handoffs** (one agent calls another)
- ✅ Can be **generic** (reusable) or **project-specific**

---

## Two-Level Workflow

### Level 1: Manual Control (speckit.implement)

**Use when**:
- Learning the codebase
- Implementing complex features
- Need to review each step
- Want full control

**How it works**:

```bash
speckit implement T-2.3
```

1. Reads task T-2.3 from tasks.md
2. Shows implementation plan
3. **Waits for your approval** at each step
4. Generates code file by file
5. You review and approve each file
6. Marks task complete when done

**Pros**:
- ✅ Full control over every decision
- ✅ Learn implementation details
- ✅ Adjust approach mid-task

**Cons**:
- ❌ Slower (requires user interaction)
- ❌ Manual validation needed

---

### Level 2: Automatic Orchestration (orchestrator)

**Use when**:
- Implementing bulk tasks
- Repetitive implementations
- Trust validation pipeline
- Need speed

**How it works**:

```
@workspace /orchestrator
```

1. Reads all 45 tasks from tasks.md
2. Executes tasks **sequentially** respecting dependencies
3. **No user approval needed** for each step
4. Automatic routing (backend → backend-implement, frontend → frontend-implement)
5. Automatic validation (SOLID review, test generation via handoffs)
6. Reports progress every 5 tasks
7. Pauses if validation fails
8. Marks tasks complete in tasks.md

**Pros**:
- ✅ Fast (no waiting for user)
- ✅ Automatic validation through handoffs
- ✅ Consistent quality (enforced SOLID, tests)

**Cons**:
- ❌ Less control (automatic execution)
- ❌ Must trust validation pipeline
- ❌ Sequential only (no parallelization)

---

## Agent Catalog

### 1. solid-reviewer (Validation Agent)

**Purpose**: Reviews C# code for SOLID principles and Clean Code compliance

**Type**: Generic (reusable across projects)

**Invocation**:
```
@workspace /solid-reviewer backend/src/Services/TournamentService.cs
```

**What it does**:
- Checks all 5 SOLID principles (SRP, OCP, LSP, ISP, DIP)
- Enforces ZERO COMMENTS policy
- Detects magic numbers, long methods, poor naming
- Returns score 0-10 with detailed violations
- Provides refactoring suggestions

**Output Example**:
```markdown
🔍 SOLID Review: TournamentService.cs

Score: 5.5/10 ⚠️

Violations:
- SRP: Service handles validation AND persistence (split responsibilities)
- Magic Number: Hardcoded 16 for max teams (extract to constant)
- Method Length: CreateTournamentAsync is 35 lines (max 20)

Recommendations:
1. Extract validation to TournamentValidator class
2. Define MaxTeamsLimit constant
3. Split CreateTournamentAsync into smaller methods
```

**Used by**:
- backend-implement (automatic handoff)
- orchestrator (via backend-implement)
- Manual invocation for code review

**Documented**: `docs/agent-skills/solid-reviewer-test-cases.md` (3 test cases for obligatorio)

---

### 2. test-generator (Test Generation Agent)

**Purpose**: Generates comprehensive MSTest unit tests with Moq

**Type**: Generic (reusable across projects)

**Invocation**:
```
@workspace /test-generator backend/src/Services/TournamentService.cs
```

**What it does**:
- Analyzes service class and methods
- Generates test file with MSTest + Moq
- Includes happy path, validation, boundary, error scenarios
- Uses AAA pattern (Arrange-Act-Assert)
- Follows naming convention: `[MethodName]_[Scenario]_[ExpectedResult]`

**Output Example**:
```markdown
✅ Tests Generated: TournamentServiceTests.cs

Coverage Summary:

CreateTournamentAsync: 5 tests
- ✅ Happy path (valid data)
- ✅ Validation (past start date, invalid end date)
- ✅ Boundary (min/max teams)
- ✅ Error scenarios (repository throws)

GetByIdAsync: 2 tests
- ✅ Existing tournament returns data
- ✅ Non-existent tournament returns null

Total Tests: 12
```

**Used by**:
- backend-implement (automatic handoff for services)
- orchestrator (via backend-implement)
- Manual invocation for test generation

---

### 3. backend-implement (Implementation Agent)

**Purpose**: Implements backend tasks with automatic validation

**Type**: Project-specific (Esports Tournament Platform)

**Invocation**:
```
@workspace /backend-implement T-2.3
```

**What it does**:
1. Reads task T-2.3 from tasks.md
2. Reads data-model.md, api-endpoints.md, constitution.md
3. Generates implementation (controller, service, repository, DTOs)
4. **Automatic handoff** to solid-reviewer for validation
5. Refactors if SOLID score < 7/10 and re-validates
6. **Automatic handoff** to test-generator for services
7. Updates task status to "completed"

**Output Example**:
```markdown
✅ Task T-2.3 Completed: Create TournamentService

Files Created/Modified:
- backend/src/Services/TournamentService.cs
- backend/src/Services/ITournamentService.cs
- backend/tests/Services/TournamentServiceTests.cs

SOLID Review: ✅ 8.5/10
- All principles satisfied
- Clean Code compliant
- ZERO COMMENTS enforced

Test Coverage: ✅ 12 tests generated
- Happy path: 3 tests
- Validation: 4 tests
- Boundary: 3 tests
- Error handling: 2 tests
```

**Handoffs**:
- → solid-reviewer (automatic)
- → test-generator (automatic for services)

**Used by**:
- orchestrator (for backend/ tasks)
- Manual invocation for single task implementation

---

### 4. frontend-implement (Implementation Agent)

**Purpose**: Implements Angular 21 frontend features

**Type**: Project-specific (Esports Tournament Platform)

**Invocation**:
```
@workspace /frontend-implement T-4.3
```

**What it does**:
1. Reads task T-4.3 from tasks.md
2. Reads api-endpoints.md, spec.md
3. Generates implementation (components, services, models)
4. Ensures TypeScript strict mode compliance
5. Applies Angular best practices (OnPush, reactive forms, standalone components)
6. Updates task status to "completed"

**Output Example**:
```markdown
✅ Task T-4.3 Completed: Create Tournament Registration Form

Files Created/Modified:
- frontend/src/app/components/create-tournament/create-tournament.component.ts
- frontend/src/app/components/create-tournament/create-tournament.component.html
- frontend/src/app/components/create-tournament/create-tournament.component.css
- frontend/src/app/services/tournament.service.ts
- frontend/src/app/models/tournament.model.ts

Type Safety: ✅ All types defined
Reactive: ✅ OnPush + Observables
Routing: ✅ Guards configured
```

**Used by**:
- orchestrator (for frontend/ tasks)
- Manual invocation for single task implementation

---

### 5. orchestrator (Coordination Agent)

**Purpose**: Coordinates execution of all 45 tasks automatically

**Type**: Project-specific (Esports Tournament Platform)

**Invocation**:
```
@workspace /orchestrator
```

**What it does**:
1. Reads all tasks from tasks.md
2. Sorts by dependencies (respects `depends` field)
3. Routes backend/ tasks → backend-implement
4. Routes frontend/ tasks → frontend-implement
5. Monitors automatic validation (via handoffs)
6. Reports progress every 5 tasks
7. Marks tasks complete in tasks.md
8. Generates final completion report

**Optional parameters**:
```
@workspace /orchestrator start-from=T-2.3
@workspace /orchestrator phase=2
@workspace /orchestrator dry-run
```

**Output Example** (Progress Report):
```markdown
📊 Orchestrator Progress Report

Phase: 2 - Foundational Backend
Completed: 5 / 8 tasks
Current Task: T-2.6 - Create TournamentRepository

Recent Completions:
- ✅ T-2.1: Setup project structure
- ✅ T-2.2: Create Tournament entity
- ✅ T-2.3: Create TournamentService (SOLID: 8.5/10, Tests: 12)
- ✅ T-2.4: Create ITournamentRepository interface
- ✅ T-2.5: Create TournamentRepository

Validation Summary:
- SOLID Scores: 8.5, 9.0, 8.0 (avg: 8.5)
- Tests Generated: 35 total

Next Tasks:
- T-2.6: Create TournamentRepository
- T-2.7: Configure DbContext
```

**Handoffs**:
- → backend-implement (for backend tasks)
- → frontend-implement (for frontend tasks)
- Indirectly triggers solid-reviewer and test-generator via backend-implement

**Error Handling**:
- Pauses on SOLID validation failure
- Reports detailed error with resolution steps
- Can resume with `@workspace /orchestrator resume`

---

## Usage Examples

### Example 1: Implement Single Backend Task Manually

**Goal**: Implement TournamentService with full control

**Steps**:

1. Use speckit.implement for manual control:
```bash
speckit implement T-2.3
```

2. Review proposed implementation
3. Approve file creation
4. Manually review generated code
5. Optionally invoke solid-reviewer:
```
@workspace /solid-reviewer backend/src/Services/TournamentService.cs
```

6. Refactor if needed
7. Optionally generate tests:
```
@workspace /test-generator backend/src/Services/TournamentService.cs
```

---

### Example 2: Implement Single Backend Task Automatically

**Goal**: Implement TournamentService with automatic validation

**Steps**:

1. Invoke backend-implement:
```
@workspace /backend-implement T-2.3
```

2. Agent automatically:
   - Reads context (data-model.md, api-endpoints.md)
   - Generates service implementation
   - Calls solid-reviewer for validation
   - Refactors if score < 7/10
   - Calls test-generator for unit tests
   - Marks task complete

3. Review final output

---

### Example 3: Orchestrate All Tasks Automatically

**Goal**: Implement all 45 tasks with automatic validation

**Steps**:

1. Start orchestrator:
```
@workspace /orchestrator
```

2. Agent automatically:
   - Reads all tasks from tasks.md
   - Executes tasks sequentially (respecting dependencies)
   - Routes to backend-implement or frontend-implement
   - Reports progress every 5 tasks
   - Generates final report

3. Monitor progress reports
4. Review final completion report

---

### Example 4: Review Existing Code

**Goal**: Check if existing code meets SOLID standards

**Steps**:

1. Invoke solid-reviewer directly:
```
@workspace /solid-reviewer backend/src/Services/TournamentService.cs
```

2. Review score and violations
3. Apply refactoring suggestions
4. Re-run solid-reviewer to verify improvements

---

### Example 5: Generate Tests for Existing Service

**Goal**: Add comprehensive tests to service without tests

**Steps**:

1. Invoke test-generator directly:
```
@workspace /test-generator backend/src/Services/TournamentService.cs
```

2. Review generated test file
3. Add to test project
4. Run tests: `dotnet test`

---

### Example 6: Resume Orchestration After Error

**Goal**: Continue from where orchestration stopped

**Steps**:

1. Orchestrator paused due to SOLID validation failure:
```markdown
🛑 Orchestrator Paused

Failed Task: T-2.3 - Create TournamentService
Error: SOLID validation failed (score: 5.5/10)
```

2. Manually fix violations or let backend-implement retry:
```
@workspace /backend-implement T-2.3
```

3. Resume orchestration:
```
@workspace /orchestrator resume
```

---

## How Agents Work

### Technical Implementation

**Agent Definition Files**:
- Location: `.github/agents/*.agent.md`
- Format: Markdown with YAML frontmatter
- Read by: GitHub Copilot Chat

**Example Structure**:
```markdown
---
description: Brief description of agent purpose
handoffs:
  - agent-name-1
  - agent-name-2
---

# Agent Name

Detailed instructions for Copilot...
```

**Invocation**:
- Via Copilot Chat: `@workspace /agent-name [parameters]`
- Copilot reads `.github/agents/agent-name.agent.md`
- Executes instructions defined in the file

**Handoffs**:
- Agent can "call" another agent by including in `handoffs` list
- Automatic in Copilot Chat (not GitHub CLI)
- Example: backend-implement → solid-reviewer → test-generator

**No External Dependencies**:
- ❌ No GitHub CLI needed
- ❌ No API calls to external services
- ❌ No custom scripts or executables
- ✅ Only GitHub Copilot + agent definition files

---

### Agent vs Tool vs Script

| Type | Example | How it Works |
|------|---------|--------------|
| **Agent** | solid-reviewer | Markdown instructions read by Copilot |
| **Tool** | speckit.implement | CLI command that calls Copilot API |
| **Script** | PowerShell/Bash | Direct code execution |

**Agents are NOT executable programs**. They are instructions that Copilot follows.

---

## Decision Guide

### When to Use speckit.implement

✅ **Use speckit.implement when**:
- You're implementing the first few tasks (learning phase)
- Task is complex or ambiguous
- You want to review every decision
- You need to adjust approach mid-implementation
- You're learning the codebase structure

❌ **Don't use speckit.implement when**:
- You have 20+ similar tasks to implement
- You trust the validation pipeline
- You need speed over control
- Tasks are straightforward and repetitive

---

### When to Use orchestrator

✅ **Use orchestrator when**:
- You have bulk tasks to implement (10+ tasks)
- Tasks are well-defined in tasks.md
- You trust automatic SOLID validation
- You need fast implementation
- Tasks are repetitive (CRUD operations, similar components)

❌ **Don't use orchestrator when**:
- You're learning the codebase
- Tasks are complex or ambiguous
- You want to review each file before creation
- You need to adjust approach frequently

---

### When to Use Individual Agents

✅ **Use individual agents when**:
- You need one specific function (review, tests, implementation)
- You want to combine manual + automatic approaches
- You're debugging a specific issue
- You want to validate existing code

**Examples**:
- `@workspace /solid-reviewer` - Review existing code
- `@workspace /test-generator` - Add tests to existing service
- `@workspace /backend-implement` - Implement single task with validation

---

### Recommended Workflow

**Phase 1: Learning (Tasks 1-10)**

1. Use **speckit.implement** for first few tasks
2. Review each generated file carefully
3. Learn patterns and structure
4. Invoke **solid-reviewer** manually to understand validation

**Phase 2: Acceleration (Tasks 11-30)**

1. Switch to **backend-implement** for single tasks
2. Trust automatic SOLID validation
3. Review output summaries
4. Manually test key features

**Phase 3: Bulk Implementation (Tasks 31-45)**

1. Use **orchestrator** for remaining tasks
2. Monitor progress reports every 5 tasks
3. Focus on integration testing
4. Review final completion report

---

## Future Architecture

### Level 3: GitHub Workflows (Not Implemented)

**Conceptual only - documented for future reference**

**How it would work**:

1. GitHub Workflow triggers on push/PR
2. Parses tasks.md for parallelizable tasks (marked with `[P]`)
3. Spawns multiple jobs in parallel
4. Each job calls Anthropic API with agent instructions
5. Aggregates results
6. Creates PR with all generated code

**Why not implemented for obligatorio**:
- ❌ Requires Anthropic API key (paid)
- ❌ Requires GitHub Actions setup
- ❌ Adds complexity (workflow YAML, API integration)
- ❌ Overkill for 45-task project
- ✅ Sequential orchestrator sufficient for obligatorio scope

**See**: `docs/future-architecture/github-workflows-guide.md` (conceptual)

---

## Obligatorio Documentation

### Agent Skills Requirements

**Obligatorio requires**:
1. ✅ Create at least 1 custom Agent Skill
2. ✅ Document 3 test cases for the skill
3. ✅ Show usage in SDLC phases: requirements, implementation, debugging, unit tests, test cases

**Our Implementation**:

1. **Custom Agent**: solid-reviewer (SOLID principles reviewer)
2. **Test Cases**: `docs/agent-skills/solid-reviewer-test-cases.md`
   - Test 1: SRP violation detection
   - Test 2: ZERO COMMENTS policy enforcement
   - Test 3: Magic numbers detection
3. **SDLC Usage**:
   - **Requirements**: Defined SOLID principles in constitution.md
   - **Implementation**: Used via backend-implement handoff for all services
   - **Debugging**: Manual invocation to identify code issues
   - **Unit Tests**: Enforced test coverage for services (via test-generator)
   - **Test Cases**: Documented in test-cases.md

**Additional Agents** (bonus):
- test-generator (generic test generation)
- backend-implement (project-specific implementation with handoffs)
- frontend-implement (Angular implementation)
- orchestrator (task coordination)

---

## Quick Reference

### Agent Invocations

```bash
# Validation
@workspace /solid-reviewer backend/src/Services/TournamentService.cs

# Test Generation
@workspace /test-generator backend/src/Services/TournamentService.cs

# Single Task (Backend)
@workspace /backend-implement T-2.3

# Single Task (Frontend)
@workspace /frontend-implement T-4.3

# All Tasks (Orchestration)
@workspace /orchestrator

# Orchestration Options
@workspace /orchestrator start-from=T-2.3
@workspace /orchestrator phase=2
@workspace /orchestrator dry-run
@workspace /orchestrator resume
```

### speckit Commands

```bash
# Manual implementation with approval steps
speckit implement T-2.3

# List all tasks
speckit tasks list

# Show task details
speckit tasks show T-2.3
```

---

## Summary

**Two-Level Workflow**:
1. **speckit.implement**: Manual control, step-by-step approval
2. **orchestrator**: Automatic sequential execution with validation

**Five Agents**:
1. **solid-reviewer**: SOLID validation (generic, reusable)
2. **test-generator**: MSTest unit test generation (generic, reusable)
3. **backend-implement**: Backend implementation with handoffs (project-specific)
4. **frontend-implement**: Angular implementation (project-specific)
5. **orchestrator**: Task coordination (project-specific)

**Key Principles**:
- ✅ Work via Copilot Chat (no external tools)
- ✅ Support handoffs (automatic validation)
- ✅ Sequential execution (no parallelization in orchestrator)
- ✅ Manual override available (speckit.implement or individual agents)
- ✅ Quality enforced (SOLID >= 7/10, comprehensive tests)

**Next Steps**:
1. Choose workflow (speckit.implement vs orchestrator)
2. Start implementation
3. Monitor validation results
4. Test manually
5. Document for obligatorio
