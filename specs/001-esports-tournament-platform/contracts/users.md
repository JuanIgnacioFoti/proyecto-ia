# Contract: Users

**Base path**: `/api/users`
**Auth required**: Bearer JWT (all endpoints)

---

## GET `/api/users/me`

Get the authenticated user's own profile.

**Auth**: Any authenticated user (Player or Organizer).

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 OK | See schemas below | Profile returned |
| 401 Unauthorized | `{ "message": "..." }` | No valid token |

**Response body — Player**:
```json
{
  "id": "guid",
  "role": "Player",
  "username": "string",
  "realName": "string",
  "email": "string",
  "mainVideogame": { "id": "guid", "name": "string" },
  "createdAt": "ISO-8601"
}
```

**Response body — Organizer**:
```json
{
  "id": "guid",
  "role": "Organizer",
  "organizationName": "string",
  "email": "string",
  "createdAt": "ISO-8601"
}
```

**FR**: FR-009

---

## PATCH `/api/users/me`

Update the authenticated user's own account.

**Auth**: Any authenticated user.

**Request body — Player** (all fields optional; omit to keep unchanged):
```json
{
  "realName": "string",    // updatable
  "email": "string",       // updatable, must be unique
  "password": "string"     // updatable
}
```

**Request body — Organizer** (all fields optional):
```json
{
  "organizationName": "string",  // updatable, 3–60 chars, unique
  "password": "string"           // updatable, complexity rules apply
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 200 OK | Updated profile (same shape as GET /me) | Update succeeded |
| 400 Bad Request | `{ "errors": { "field": ["message"] } }` | Validation error |
| 401 Unauthorized | `{ "message": "..." }` | No valid token |
| 409 Conflict | `{ "message": "..." }` | Email or organization name already in use |

**Notes**:
- Players cannot change `username` or `mainVideogame` (FR-007).
- Organizers cannot change `email` (FR-037).

**FR**: FR-038, FR-037

---

## GET `/api/users` *(Admin only)*

List all users.

**Auth**: Admin role required.

**Query parameters** (all optional):
| Param | Type | Description |
|-------|------|-------------|
| `role` | string | Filter by role: `Player`, `Organizer`, `Admin` |
| `page` | int | Page number (1-based, default 1) |
| `pageSize` | int | Items per page (default 20, max 100) |

**Response**:
```json
{
  "items": [
    {
      "id": "guid",
      "email": "string",
      "role": "string",
      "isActive": true,
      "createdAt": "ISO-8601"
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | List returned |
| 401 Unauthorized | No valid token |
| 403 Forbidden | Caller is not Admin |

**FR**: FR-008

---

## PATCH `/api/users/{userId}/role` *(Admin only)*

Change a user's role.

**Auth**: Admin role required.

**Path parameter**: `userId` — Guid

**Request body**:
```json
{
  "role": "Player" | "Organizer"
}
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Role updated |
| 400 Bad Request | Attempt to assign Admin role |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller not Admin; or attempt to change another Admin's role |
| 404 Not Found | User not found |

**FR**: FR-008

---

## POST `/api/users/{userId}/suspend` *(Admin only)*

Suspend an Organizer account (and move their non-Completed tournaments to `Suspended`).

**Auth**: Admin role required.

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Account suspended |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller not Admin |
| 404 Not Found | User not found |
| 422 Unprocessable Entity | User is not an Organizer or already suspended |

**FR**: FR-008

---

## POST `/api/users/{userId}/reinstate` *(Admin only)*

Reinstate a suspended Organizer account (and restore their tournaments to pre-suspension status).

**Auth**: Admin role required.

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Account reinstated |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller not Admin |
| 404 Not Found | User not found |
| 422 Unprocessable Entity | User is not suspended |

**FR**: FR-008
