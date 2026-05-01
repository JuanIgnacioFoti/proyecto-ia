# Contract: Matches

**Base path**: `/api/tournaments/{tournamentId}/matches`
**Auth required**: Bearer JWT (all write endpoints); GET endpoints are public (FR-015)

---

## GET `/api/tournaments/{tournamentId}/matches`

List all match results for a tournament.

**Auth**: None required.

**Response**:
```json
[
  {
    "id": "guid",
    "homeTeam": { "id": "guid", "name": "string" },
    "awayTeam": { "id": "guid", "name": "string" },
    "homeScore": 2,
    "awayScore": 1,
    "playedAt": "ISO-8601",
    "recordedAt": "ISO-8601"
  }
]
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | List returned (empty array if no matches yet) |
| 404 Not Found | Tournament not found |

**FR**: FR-015

---

## POST `/api/tournaments/{tournamentId}/matches`

Record a new match result.

**Auth**: Organizer (must own the tournament).

**Request body**:
```json
{
  "homeTeamId": "guid",
  "awayTeamId": "guid",
  "homeScore": 2,       // non-negative integer
  "awayScore": 1,       // non-negative integer
  "playedAt": "ISO-8601"
}
```

**Notes**:
- `homeTeamId` and `awayTeamId` must both be registered in the tournament.
- `homeTeamId ≠ awayTeamId`.
- Duplicate detection: reject if a match with the same two teams (any home/away order) at the same `playedAt` already exists (FR-035).
- After persisting, standings are fully recalculated and a SignalR push is sent to `tournament-{tournamentId}` group (FR-026, FR-028).

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 Created | `{ "id": "guid" }` | Match recorded; standings recalculated |
| 400 Bad Request | `{ "errors": { ... } }` | Validation error (negative score, missing fields) |
| 401 Unauthorized | | No token |
| 403 Forbidden | | Not the owning Organizer |
| 404 Not Found | | Tournament or team not found |
| 409 Conflict | `{ "message": "..." }` | Duplicate match (same teams + same playedAt) |
| 422 Unprocessable Entity | `{ "message": "..." }` | Tournament not InProgress; teams not registered; `homeTeamId` equals `awayTeamId` |

**FR**: FR-025, FR-026, FR-028, FR-035

---

## PUT `/api/tournaments/{tournamentId}/matches/{matchId}`

Correct a previously recorded match result.

**Auth**: Organizer (must own the tournament).

**Request body**:
```json
{
  "homeScore": 3,
  "awayScore": 0,
  "playedAt": "ISO-8601"   // optional; update if correcting the datetime
}
```

**Notes**:
- Allowed only while tournament status is not `Completed`.
- After persisting, a **full standings recalculation** is triggered and a SignalR push is sent (FR-027, FR-028).

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Result corrected; standings recalculated |
| 400 Bad Request | Validation error |
| 401 Unauthorized | No token |
| 403 Forbidden | Not the owning Organizer |
| 404 Not Found | Match or tournament not found |
| 409 Conflict | Corrected datetime creates a duplicate (same teams + same new playedAt) |
| 422 Unprocessable Entity | Tournament is Completed |

**FR**: FR-027, FR-028
