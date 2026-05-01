# Contract: Authentication

**Base path**: `/api/auth`
**Auth required**: No (all endpoints are public)

---

## POST `/api/auth/register/player`

Register a new Player account.

**Request body**:
```json
{
  "username": "string",       // required, unique, immutable
  "realName": "string",       // required
  "email": "string",          // required, unique, valid email format
  "password": "string",       // required
  "mainVideogameId": "guid"   // required, must exist in Videogame catalog
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 Created | `{ "userId": "guid" }` | Account created successfully |
| 400 Bad Request | `{ "errors": { "field": ["message"] } }` | Validation error (missing field, invalid email, etc.) |
| 409 Conflict | `{ "message": "..." }` | Username or email already in use |

**FR**: FR-001

---

## POST `/api/auth/register/organizer`

Register a new Organizer account.

**Request body**:
```json
{
  "organizationName": "string",  // required, unique, 3–60 characters
  "email": "string",             // required, unique, valid email format
  "password": "string"           // required, ≥8 chars, ≥1 uppercase, ≥1 lowercase, ≥1 digit
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 Created | `{ "userId": "guid" }` | Account created successfully |
| 400 Bad Request | `{ "errors": { "field": ["message"] } }` | Validation error |
| 409 Conflict | `{ "message": "..." }` | Organization name or email already in use |

**FR**: FR-036

---

## POST `/api/auth/login`

Authenticate and receive a JWT token.

**Request body**:
```json
{
  "email": "string",
  "password": "string"
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 OK | `{ "token": "string", "expiresAt": "ISO-8601" }` | Credentials valid |
| 401 Unauthorized | `{ "message": "Invalid credentials" }` | Wrong email or password |

**Notes**: Token lifetime = 24 hours (FR-002). Token claims: `sub` (userId), `email`, `role`.

**FR**: FR-002
