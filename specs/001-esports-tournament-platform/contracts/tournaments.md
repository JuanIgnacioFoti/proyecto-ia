# Contract: Tournaments

**Base path**: `/api/tournaments`

---

## GET `/api/tournaments`

List tournaments. Visibility depends on auth status.

**Auth**: Optional.

**Behavior**:
- Unauthenticated: returns `Open`, `InProgress`, `Completed`, and `Suspended` tournaments.
- Authenticated non-owning user (Player or other Organizer): same as unauthenticated.
- Authenticated owning Organizer: additionally returns their own `Draft` tournaments.

**Query parameters** (all optional):
| Param | Type | Description |
|-------|------|-------------|
| `status` | string | Filter by status |
| `videogameId` | guid | Filter by videogame |
| `page` | int | Page (1-based, default 1) |
| `pageSize` | int | Default 20, max 100 |

**Response**:
```json
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "videogame": { "id": "guid", "name": "string" },
      "organizerName": "string",
      "status": "Open",
      "startDate": "ISO-8601",
      "estimatedEndDate": "ISO-8601",
      "registeredTeamsCount": 0,
      "maxTeams": 16
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

**FR**: FR-015

---

## GET `/api/tournaments/{id}`

Get tournament details.

**Auth**: Optional. `Draft` tournaments only visible to their owning Organizer.

**Response**:
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "videogame": { "id": "guid", "name": "string" },
  "organizerName": "string",
  "status": "string",
  "startDate": "ISO-8601",
  "estimatedEndDate": "ISO-8601",
  "maxTeams": 16,
  "minMembersPerTeam": 5,
  "scoringSystem": {
    "type": "Standard",
    "winPoints": 3,
    "drawPoints": 1,
    "lossPoints": 0
  },
  "registeredTeams": [
    { "teamId": "guid", "teamName": "string", "registeredAt": "ISO-8601" }
  ],
  "matchHistory": [
    {
      "id": "guid",
      "homeTeam": { "id": "guid", "name": "string" },
      "awayTeam": { "id": "guid", "name": "string" },
      "homeScore": 2,
      "awayScore": 1,
      "playedAt": "ISO-8601"
    }
  ]
}
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Tournament found and caller has visibility |
| 404 Not Found | Not found or Draft not owned by caller |

**FR**: FR-015

---

## POST `/api/tournaments`

Create a new tournament.

**Auth**: Organizer role required.

**Request body**:
```json
{
  "name": "string",
  "videogameId": "guid",
  "description": "string",
  "startDate": "ISO-8601",           // must be strictly in the future
  "estimatedEndDate": "ISO-8601",    // must be after startDate
  "maxTeams": 16,                    // ≥ 2
  "minMembersPerTeam": 5,            // ≥ 1, default 5
  "scoringSystem": {
    "type": "Standard" | "WinnerTakesAll" | "Custom",
    "winPoints": 3,    // required only when type = Custom, ≥ 0
    "drawPoints": 1,   // required only when type = Custom, ≥ 0
    "lossPoints": 0    // required only when type = Custom, ≥ 0
  }
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 Created | `{ "id": "guid" }` | Tournament created in Draft status |
| 400 Bad Request | `{ "errors": { ... } }` | Validation error |
| 401 Unauthorized | | No token |
| 403 Forbidden | | Caller is not Organizer |

**FR**: FR-010, FR-029, FR-030

---

## PUT `/api/tournaments/{id}`

Update editable tournament fields.

**Auth**: Organizer (owner only).

**Editable when status is Draft or Open**:
```json
{
  "name": "string",
  "description": "string",
  "startDate": "ISO-8601",
  "estimatedEndDate": "ISO-8601",
  "maxTeams": 16,
  "minMembersPerTeam": 5,
  "scoringSystem": {
    "type": "Standard" | "WinnerTakesAll" | "Custom",
    "winPoints": 3,
    "drawPoints": 1,
    "lossPoints": 0
  }
}
```

**Notes**: `scoringSystem` changes are rejected once status is `InProgress` or later (FR-032).

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Updated |
| 400 Bad Request | Validation error |
| 401 Unauthorized | No token |
| 403 Forbidden | Not the owning Organizer |
| 404 Not Found | Tournament not found |
| 422 Unprocessable Entity | Attempted to change scoring system after InProgress |

**FR**: FR-011, FR-032

---

## POST `/api/tournaments/{id}/advance`

Advance the tournament status.

**Auth**: Organizer (owner only).

**Request body**:
```json
{
  "targetStatus": "Open" | "InProgress" | "Completed"
}
```

**Notes**:
- `targetStatus` must be a forward state relative to current status.
- Cannot revert to a previous state.
- `Suspended` state is not a valid `targetStatus` for this endpoint.

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Status advanced |
| 401 Unauthorized | No token |
| 403 Forbidden | Not the owning Organizer |
| 404 Not Found | Tournament not found |
| 422 Unprocessable Entity | Invalid transition (backward or invalid target) |

**FR**: FR-012
