# Contract: Standings

**Base path**: `/api/tournaments/{tournamentId}/standings`
**Auth required**: Yes — authenticated users only (FR-039)

---

## GET `/api/tournaments/{tournamentId}/standings`

Get the current standings table for a tournament.

**Auth**: Any authenticated user (Player or Organizer or Admin).

**Notes**:
- Only available for tournaments in `InProgress` or `Completed` status.
- Position (rank) is computed server-side by ordering on `points DESC`; teams with equal points share the same rank.
- Anonymous visitors are **not** permitted to view standings (FR-039).

**Response**:
```json
[
  {
    "position": 1,
    "team": { "id": "guid", "name": "string" },
    "points": 9,
    "matchesPlayed": 3,
    "wins": 3,
    "draws": 0,
    "losses": 0
  },
  {
    "position": 2,
    "team": { "id": "guid", "name": "string" },
    "points": 6,
    "matchesPlayed": 3,
    "wins": 2,
    "draws": 0,
    "losses": 1
  },
  {
    "position": 3,
    "team": { "id": "guid", "name": "string" },
    "points": 3,
    "matchesPlayed": 3,
    "wins": 1,
    "draws": 0,
    "losses": 2
  },
  {
    "position": 3,
    "team": { "id": "guid", "name": "string" },
    "points": 3,
    "matchesPlayed": 3,
    "wins": 1,
    "draws": 0,
    "losses": 2
  }
]
```

**Note on tied positions**: Two teams with the same `points` receive the **same `position` value** (e.g., both `3`). No secondary tiebreaker is applied (FR-031).

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Standings returned |
| 401 Unauthorized | Caller is not authenticated |
| 404 Not Found | Tournament not found |
| 422 Unprocessable Entity | Tournament is in `Draft`, `Open`, or `Suspended` status |

**FR**: FR-039, FR-031

---

## SignalR Real-Time Updates

**Hub URL**: `/hubs/tournament`
**Auth**: Bearer JWT required to establish connection (anonymous connections are rejected — standings are auth-gated per FR-039).
**Group**: `tournament-{tournamentId}` (client must call `JoinTournamentGroup(tournamentId)` after connecting)

**Server → Client method**: `ReceiveStandingsUpdate`

**Payload**:
```json
{
  "tournamentId": "guid",
  "standings": [
    {
      "position": 1,
      "team": { "id": "guid", "name": "string" },
      "points": 9,
      "matchesPlayed": 3,
      "wins": 3,
      "draws": 0,
      "losses": 0
    }
  ],
  "latestMatch": {
    "id": "guid",
    "homeTeam": { "id": "guid", "name": "string" },
    "awayTeam": { "id": "guid", "name": "string" },
    "homeScore": 2,
    "awayScore": 1,
    "playedAt": "ISO-8601"
  }
}
```

**Notes**:
- Pushed to all clients in the `tournament-{tournamentId}` SignalR group after every match record or correction.
- Must reach connected clients within 5 seconds of the Organizer recording the result (SC-004).
- Angular `SignalRService` subscribes to this method and updates the standings table and match list in place without a full page reload (FR-028).

**FR**: FR-028
