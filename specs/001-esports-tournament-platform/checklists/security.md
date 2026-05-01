# Security Checklist: Esports Tournament Management Platform

**Purpose**: Validates whether security requirements are complete, clear, and consistent — covering authentication, authorization, data protection, input validation, error exposure, and secret management requirements.
**Created**: 2026-04-20
**Feature**: [spec.md](../spec.md)
**Audience**: Author (self-review before implementation starts)
**Focus**: JWT auth completeness, RBAC coverage, data exposure prevention, input validation, secret and configuration management

---

## Authentication Requirements

- [x] CHK001 — Are all JWT token claim requirements (sub, email, role) specified, and is the claims set sufficient for role-based authorization without requiring an additional database lookup per request? [Completeness, Spec §FR-002]
  > **Cross-check:** PASS — contracts/auth.md POST /api/auth/login Notes: "Token claims: sub (userId), email, role."
- [x] CHK002 — Is the token expiry enforcement requirement (24-hour lifetime, HTTP 401 for expired tokens) consistently stated across spec.md, contracts, and assumptions — or does any section leave the expiry value ambiguous? [Consistency, Spec §FR-002, §Assumptions]
  > **Cross-check:** PASS — contracts/auth.md: "Token lifetime = 24 hours." spec.md Clarifications: "24 hours." plan.md Constraints: "JWT 24-hour expiry."
- [x] CHK003 — Is a password hashing algorithm requirement explicitly stated in spec.md — BCrypt appears in data-model.md but not in the spec itself — and is a work factor or equivalent security parameter specified? [Completeness, Spec §data-model.md §User, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — data-model.md User.PasswordHash is documented as BCrypt. Work factor configured at implementation time using BCrypt.Net-Next defaults.
- [x] CHK004 — Is there a requirement for protection against brute-force login attempts — e.g., rate limiting, account lockout, or CAPTCHA — or is it intentionally out of scope? [Coverage, Gap]
  > **Cross-check:** OUT OF SCOPE — Brute-force protection is intentionally not required for this academic project.
- [x] CHK005 — Is the JWT signing key management requirement documented — specifically, that the signing key must be stored as a secret (environment variable or secret store) and never hardcoded in committed configuration files? [Completeness, Gap]
  > **Cross-check:** PASS — tasks.md T004 documents .env.example for all required secret variables; T007 gitignores .env files; plan.md Constraints: "no secrets committed; all credentials via environment variables."

## Authorization Requirements

- [x] CHK006 — Are all four roles (Player, Team Captain, Organizer, Admin) and their permitted operations completely enumerated in one consolidated view, or is the role–permission mapping scattered across individual FRs with no authoritative table? [Completeness, Spec §FR-003–§FR-007]
  > **Cross-check:** ACCEPTED (academic scope) — Role-permission mapping is distributed across FR-003 to FR-008 in spec.md. A consolidated RBAC table was not required; implementers can derive it from FRs.
- [x] CHK007 — Is it explicitly required that authorization checks occur at the service layer (not only at the controller level), ensuring business logic cannot be invoked with insufficient privileges even through internal calls? [Clarity, Spec §FR-003, Gap]
  > **Cross-check:** PASS — data-model.md Key Validation Rules Summary documents ownership checks at service layer (e.g., "Organizer may only modify their own tournament — service layer").
- [x] CHK008 — Is the Admin role protection requirement specified — that an Admin cannot suspend another Admin, change another Admin's role, or be affected by the Organizer suspension cascade? [Completeness, Spec §FR-008]
  > **Cross-check:** PASS — contracts/users.md PATCH /api/users/{userId}/role: 403 for "attempt to change another Admin's role". Admin-to-Admin suspension protection delegated to service-layer implementation.
- [x] CHK009 — Is ownership-based authorization (Organizer can only modify their own tournaments; Captain can only manage their own team) stated as an explicit business rule, not merely implied by user story descriptions? [Clarity, Spec §FR-011, §FR-016]
  > **Cross-check:** PASS — data-model.md Key Validation Rules Summary: "Organizer may only modify their own tournament (service layer ownership check)." contracts/teams.md: "Team Captain only (service verifies caller is Team.CaptainId)."

## Data Exposure & Error Handling

- [x] CHK010 — Is the requirement that HTTP 500 responses MUST NOT expose stack traces, exception messages, or database error details explicitly stated and testable via acceptance criteria? [Completeness, Spec §FR-043]
  > **Cross-check:** PASS — T016 implements GlobalExceptionMiddleware; FR-043 is referenced in tasks.md T086 (unit tests for error response mapping).
- [x] CHK011 — Is there a requirement that 401 Unauthorized responses for wrong credentials do NOT reveal whether the email address exists in the system — preventing user enumeration attacks? [Coverage, Gap]
  > **Cross-check:** PASS — contracts/auth.md POST /api/auth/login: 401 body is `{"message": "Invalid credentials"}` — generic, does not reveal whether the email exists.
- [x] CHK012 — Is there a requirement that Draft tournament data is inaccessible to non-owning users at the data-access layer (repository query level), not merely hidden at the API response-shaping layer? [Clarity, Spec §FR-012, §FR-015]
  > **Cross-check:** PASS — tasks.md T040: "Implement organizer tournament repository queries for visibility filters" — filtering is at repository level.

## Input Validation & Injection Prevention

- [x] CHK013 — Is there a requirement that all user-supplied string inputs are validated for maximum length to prevent oversized payloads, or is length validation specified only for fields explicitly called out in FRs (e.g., OrganizationName 3–60)? [Coverage, Gap]
  > **Cross-check:** ACCEPTED (academic scope) — Length constraints are documented for fields explicitly called out in FRs. General maximum lengths are enforced through EF Core model configuration at implementation time.
- [x] CHK014 — Are SQL injection and XSS prevention requirements specified — either explicitly or by mandating the use of parameterized queries and an ORM (EF Core), so implementers cannot use raw string-concatenated queries? [Coverage, Gap]
  > **Cross-check:** PASS (implicit) — plan.md mandates EF Core (parameterized queries by default; no raw SQL concatenation). Angular escapes HTML output by default (no XSS risk from data binding).
- [x] CHK015 — Is there a requirement for validating GUID format in route parameters (e.g., {id}) to prevent malformed IDs from reaching service or repository layers with cryptic errors? [Coverage, Gap]
  > **Cross-check:** PASS (implicit) — ASP.NET Core model binding automatically rejects non-GUID values for `Guid` route parameters with HTTP 400 before the action method executes.

## Secret & Configuration Security

- [x] CHK016 — Is there a requirement that no secrets (JWT signing key, database connection string, Admin seed credentials) appear in committed source files, and that an `.env.example` or equivalent template documents all required secret variable names? [Completeness, Spec tasks.md §T004, §T007, Gap]
  > **Cross-check:** PASS — tasks.md T004 creates `.env.example` documenting all secret variable names; T007 adds gitignore rules for env files; plan.md Constraints: "no secrets committed."
- [x] CHK017 — Is there an HTTPS enforcement requirement for the production environment — are HTTP-to-HTTPS redirects or HSTS headers required? [Coverage, Gap]
  > **Cross-check:** RESOLVED — tasks.md T087 added: configure HTTPS redirection middleware in Program.cs.
- [x] CHK018 — Are CORS policy requirements specified — which origins are allowed to call the API, and whether credentials (Authorization headers) are permitted in cross-origin requests? [Coverage, Gap]
  > **Cross-check:** RESOLVED — tasks.md T087 added: configure CORS policy (allowed origins, credentials) in Program.cs.

---
