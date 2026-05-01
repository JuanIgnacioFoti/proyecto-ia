# Feature Specification: Registration and Login

**Feature**: `001-registration-and-login`  
**Parent**: `001-esports-tournament-platform`  
**Created**: 2026-04-24  
**Status**: Final  
**Priority**: P1

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Player Self-Registration (Priority: P1)

A visitor registers as a Player by providing a unique username, their real name, a unique valid email address, a password, and their main videogame. Upon successful registration the account is immediately active and the user can log in.

**Why this priority**: Player registration is the entry point for the most numerous user group on the platform. No team can be formed, no tournament can be entered, and no results can be viewed (authenticated-only) until a Player has an account.

**Independent Test**: Register one Player with all valid fields; verify the account is created and the user can log in immediately. Attempt to register a second Player with the same username or email; verify the system returns a validation error identifying the duplicate field.

**Acceptance Scenarios**:

1. **Given** a visitor with no account, **When** they submit valid Player registration data (unique username, real name, unique valid email, password, and main videogame), **Then** a Player account is created and they can immediately log in.
2. **Given** a visitor attempting to register as a Player with a username already in use, **When** they submit the form, **Then** the system rejects the request with a validation error identifying the duplicate username.
3. **Given** a visitor attempting to register as a Player with an email already in use, **When** they submit the form, **Then** the system rejects the request with a validation error identifying the duplicate email.
4. **Given** a visitor omitting a required field (username, real name, email, password, or main videogame), **When** they submit the form, **Then** the system rejects the request with a validation error identifying each missing field.

---

### User Story 2 - Organizer Self-Registration (Priority: P1)

A visitor registers as an Organizer by providing a unique organization name (3–60 characters), a unique valid email address, and a password that meets complexity requirements (at least 8 characters, one uppercase, one lowercase, one digit). Upon successful registration the account is immediately active.

**Why this priority**: Organizer registration is the entry point for the users who create and run tournaments. No tournament can exist without at least one Organizer.

**Independent Test**: Register one Organizer with all valid fields; verify the account is created and the user can log in. Attempt to register with a duplicate organization name, a password that violates complexity rules, and an organization name shorter than 3 characters; verify each is rejected with an appropriate message.

**Acceptance Scenarios**:

1. **Given** a visitor, **When** they submit valid Organizer registration data (unique organization name between 3 and 60 characters, unique valid email, and a password containing at least 8 characters with at least one uppercase letter, one lowercase letter, and one digit), **Then** an Organizer account is created and they can immediately log in.
2. **Given** a visitor attempting to register with an organization name already in use, **When** they submit the form, **Then** the system rejects the request with a validation error identifying the duplicate name.
3. **Given** a visitor attempting to register with an email already in use, **When** they submit the form, **Then** the system rejects the request with a validation error identifying the duplicate email.
4. **Given** a visitor attempting to register as an Organizer with a password shorter than 8 characters or missing a required character type (uppercase, lowercase, digit), **When** they submit the form, **Then** the system rejects the request with a validation error describing the violated rule.
5. **Given** a visitor attempting to register with an organization name shorter than 3 characters or longer than 60 characters, **When** they submit the form, **Then** the system rejects the request with a validation error.

---

### User Story 3 - Login and Session Management (Priority: P1)

Any registered user (Player or Organizer) logs in with their email and password. On success they receive a JWT session token valid for 24 hours. Subsequent requests include the token to access protected resources. An expired or tampered token is rejected with HTTP 401.

**Why this priority**: All role-protected operations on the platform depend on a valid session token. Without working login and token validation, no downstream feature can enforce access control.

**Independent Test**: Log in with correct credentials; verify a token is returned. Access a protected endpoint with the token; verify HTTP 200. Access the same endpoint without a token; verify HTTP 401. Access an endpoint requiring a different role; verify HTTP 403. Use an expired or tampered token; verify HTTP 401.

**Acceptance Scenarios**:

1. **Given** a registered user, **When** they submit correct credentials (email and password), **Then** they receive a valid JWT session token that allows access to protected resources.
2. **Given** a registered user, **When** they submit incorrect credentials, **Then** login is refused and no token is issued.
3. **Given** no session token, **When** the user attempts to access any protected resource, **Then** the system returns HTTP 401.
4. **Given** a valid session token, **When** the user accesses a resource requiring a role they do not hold, **Then** the system returns HTTP 403.
5. **Given** an expired or tampered token, **When** it is used for a request, **Then** the system returns HTTP 401.

---

### User Story 4 - Profile Management (Priority: P2)

An authenticated Player can update their own real name, email address, and password. An authenticated Organizer can update their organization name and password. Neither can change fields locked after registration (Player: username, main videogame; Organizer: email).

**Why this priority**: Profile updates are a standard account-hygiene feature. They do not block any other story and can be delivered independently after authentication is working.

**Independent Test**: Log in as a Player; update real name and email; verify changes are persisted. Attempt to change the username or main videogame; verify the system rejects the attempt or ignores the field. Log in as an Organizer; update the organization name to a new unique value; verify the change is persisted.

**Acceptance Scenarios**:

1. **Given** an authenticated Player, **When** they update their real name, email, or password with valid values, **Then** the changes are persisted and take effect on the next login if the email was changed.
2. **Given** an authenticated Player, **When** they attempt to change their username or main videogame, **Then** the system ignores or rejects those fields.
3. **Given** an authenticated Organizer, **When** they update their organization name to a new unique value or change their password with a valid value, **Then** the changes are persisted.
4. **Given** an authenticated Organizer, **When** they attempt to change their email, **Then** the system ignores or rejects that field.
5. **Given** an authenticated Organizer attempting to change their organization name to one already in use, **When** they submit the update, **Then** the system rejects the change with a validation error identifying the duplicate name.

---

### User Story 5 - Admin User Management (Priority: P3)

An Admin can list all registered users, change their roles (except altering another Admin's Admin role), and suspend or reinstate Organizer accounts. Suspending an Organizer automatically moves all their non-completed tournaments into a Suspended state.

**Why this priority**: Admin capabilities are a governance layer on top of the already-working user accounts. They can be implemented and tested independently once user registration and login are functional.

**Independent Test**: Log in as Admin; list all users; suspend an Organizer account; verify the Organizer can no longer log in (or is denied protected operations); verify their active tournaments are marked Suspended; reinstate the Organizer; verify their tournaments return to their pre-suspension state.

**Acceptance Scenarios**:

1. **Given** an authenticated Admin, **When** they request the user list, **Then** all registered users are returned.
2. **Given** an authenticated Admin, **When** they change a non-Admin user's role to a valid role, **Then** the change is persisted and the user's access rights reflect the new role on their next request.
3. **Given** an authenticated Admin, **When** they suspend an Organizer account, **Then** the Organizer's account is marked suspended and all their tournaments that are not yet Completed automatically enter a Suspended state.
4. **Given** an authenticated Admin, **When** they reinstate a previously suspended Organizer, **Then** the Organizer's account is restored and their tournaments return to the states they held before suspension.
5. **Given** an authenticated Admin, **When** they attempt to change the Admin role of another Admin, **Then** the system rejects the change.

---

### Edge Cases

- What happens when a Player attempts to access the Organizer registration flow (or vice versa)? → The two registration flows are separate endpoints; each validates only the fields for its own role. Accessing the wrong flow simply means the account is created with the wrong role, which is a user error; the system does not cross-validate.
- What happens when an Admin suspends an Organizer and then reinstates them after some of their previously Suspended tournaments have been manually completed? → Only tournaments still in the Suspended state are restored to their pre-suspension state; tournaments already Completed remain Completed.
- What happens when a Player changes their email to one already in use by another account? → The system rejects the update with a validation error identifying the duplicate email.
- What happens when a registered user's JWT token is used after 24 hours? → The token is expired and the system returns HTTP 401.

---

## Requirements *(mandatory)*

### Functional Requirements

#### Player Registration

- **FR-001**: The system MUST allow visitors to self-register as a **Player** by providing: unique username, real name, unique email address (valid format), password, and main videogame.
- **FR-006**: The system MUST support four distinct roles: **Player**, **Team Captain**, **Organizer**, and **Admin**. A Player who creates a team automatically assumes the Team Captain role for that team.
- **FR-007**: Users self-register into one of two roles: **Player** (via the player registration flow) or **Organizer** (via the organizer registration flow). The **Team Captain** role is automatically assumed by a Player who creates a team and is not a separate registration. The **Admin** role is reserved and cannot be obtained through self-registration.

#### Organizer Registration

- **FR-036**: The system MUST allow visitors to self-register as an **Organizer** by providing:
  - **Organization name**: unique across all Organizer accounts, minimum 3 characters, maximum 60 characters.
  - **Email address**: unique across all accounts, must be a valid email format.
  - **Password**: minimum 8 characters, must contain at least one uppercase letter, at least one lowercase letter, and at least one digit.

#### Authentication

- **FR-002**: The system MUST authenticate users via a JWT token-based mechanism; the token must be included in subsequent requests to access protected resources. JWT tokens MUST expire after **24 hours** from the time of issuance; an expired token MUST be rejected with HTTP 401.
- **FR-003**: The system MUST enforce role-based access control; each endpoint that requires authorization must validate the caller's role before processing the request.
- **FR-004**: Any request to a protected resource without a valid authentication token MUST be rejected with HTTP 401.
- **FR-005**: Any request to a resource for which the authenticated user lacks the required role MUST be rejected with HTTP 403.

#### Profile Management

- **FR-009**: A **Player** MUST be able to view their own profile.
- **FR-038**: A **Player** MUST be able to modify their own account, limited to: real name, email address (subject to the same uniqueness and format rules as FR-001), and password. Username and main videogame cannot be changed after registration.
- **FR-037**: An **Organizer** MUST be able to modify their own account, limited to: organization name (subject to the same uniqueness and length rules as FR-036) and password (subject to the same complexity rules as FR-036). The email address cannot be changed after registration.

#### Admin User Management

- **FR-008**: An **Admin** MUST be able to list all users, change their roles (except the Admin role of other Admins), and suspend or reinstate Organizer accounts. Suspending an Organizer MUST automatically move all of that Organizer's tournaments that are not yet Completed into a **Suspended** state. Reinstating the Organizer returns those tournaments to their pre-suspension state.

### Key Entities

- **User**: Abstract representation of any registered account. Each User is either a Player or an Organizer; both share email and password credentials but have distinct registration flows and profile attributes.
- **Player**: A User who participates competitively. Holds: unique username, real name, unique email, password, and main videogame. Automatically assumes the Team Captain role for any team they create.
- **Organizer**: A User who manages tournaments. Identified by a unique organization name (3–60 characters) and email address. Cannot simultaneously hold the Player role.

---

## Success Criteria *(mandatory)*

- **SC-001**: A new user can complete registration and log in within 2 minutes.
- **SC-005**: Role-based access control is enforced with 100% accuracy — no operation is performed by a user who lacks the required role.
- **SC-006**: All unauthenticated requests to protected resources receive HTTP 401, and all role-insufficient requests receive HTTP 403, in 100% of cases.

---

## Testing Requirements

- **TR-001 — Player Account Registration** (FR-001, FR-038): Unit tests MUST cover all field validation rules (username uniqueness, email format and uniqueness, password rules, required fields). Manual test cases MUST cover the complete registration flow from the user interface, including validation feedback and successful account creation.

---

## Assumptions

- **Organizer registration**: Organizers self-register through a dedicated registration flow and are immediately granted the Organizer role upon successful validation. No Admin approval is required. A single account cannot hold both the Player and Organizer roles simultaneously.
- **Admin bootstrapping**: At least one Admin account exists at system startup (seeded or configured via environment). Admin accounts cannot be created through the registration flow.
