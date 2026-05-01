# Contract: Teams

**Base path**: `/api/teams`
**Auth required**: Bearer JWT (all write endpoints); GET endpoints may be public

---

## POST `/api/teams`

Create a new team. The authenticated Player automatically becomes the Captain.

**Auth**: Player role required.

**Request body**:
```json
{
  "name": "string",         // unique within the videogame
  "description": "string",  // optional
  "videogameId": "guid"     // must match Player's mainVideogame
}
```

**Responses**:
| Status | Body | Condition |
|--------|------|-----------|
| 201 Created | `{ "id": "guid" }` | Team created; caller is now Captain |
| 400 Bad Request | `{ "errors": { ... } }` | Validation error |
| 401 Unauthorized | | No token |
| 403 Forbidden | | Caller is not a Player |
| 409 Conflict | `{ "message": "..." }` | Team name already exists for this videogame |
| 422 Unprocessable Entity | `{ "message": "..." }` | Caller is already Captain of a team for this videogame |

**FR**: FR-016

---

## GET `/api/teams/{id}`

Get team details including roster.

**Auth**: Optional.

**Response**:
```json
{
  "id": "guid",
  "name": "string",
  "description": "string",
  "videogame": { "id": "guid", "name": "string" },
  "captain": { "id": "guid", "username": "string" },
  "members": [
    { "playerId": "guid", "username": "string", "joinedAt": "ISO-8601" }
  ],
  "createdAt": "ISO-8601"
}
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Team found |
| 404 Not Found | Team not found |

---

## POST `/api/teams/{id}/invitations`

Invite a Player to the team.

**Auth**: Team Captain only (service verifies caller is `Team.CaptainId`).

**Request body**:
```json
{
  "usernameOrEmail": "string"   // identifier of the Player to invite
}
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 201 Created | Invitation sent |
| 400 Bad Request | Target Player not found by identifier |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller is not the Team Captain |
| 409 Conflict | Player already a member or already has a pending invitation |
| 422 Unprocessable Entity | Player already an active member of a team for this videogame |

**FR**: FR-017

---

## GET `/api/teams/{id}/invitations`

List pending invitations for a team (for the Captain's dashboard).

**Auth**: Team Captain only.

**Response**:
```json
[
  {
    "id": "guid",
    "invitedPlayer": { "id": "guid", "username": "string" },
    "status": "Pending",
    "createdAt": "ISO-8601",
    "expiresAt": "ISO-8601"
  }
]
```

---

## GET `/api/players/me/invitations`

List the authenticated Player's pending team invitations.

**Auth**: Player role required.

**Response**:
```json
[
  {
    "id": "guid",
    "team": { "id": "guid", "name": "string", "videogame": { "id": "guid", "name": "string" } },
    "status": "Pending",
    "createdAt": "ISO-8601",
    "expiresAt": "ISO-8601"
  }
]
```

**FR**: FR-018

---

## POST `/api/players/me/invitations/{invitationId}/accept`

Accept a team invitation.

**Auth**: Player role required (must be the invited Player).

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Invitation accepted; Player is now a team member |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller is not the invited Player |
| 404 Not Found | Invitation not found |
| 422 Unprocessable Entity | Invitation expired or already processed; or Player already a member of a team for this videogame |

**FR**: FR-018, FR-019

---

## POST `/api/players/me/invitations/{invitationId}/decline`

Decline a team invitation.

**Auth**: Player role required (must be the invited Player).

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Invitation declined |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller is not the invited Player |
| 404 Not Found | Invitation not found |
| 422 Unprocessable Entity | Invitation expired or already processed |

**FR**: FR-018

---

## DELETE `/api/teams/{id}/members/{playerId}`

Remove a member from the team.

**Auth**: Team Captain only.

**Responses**:
| Status | Condition |
|--------|-----------|
| 204 No Content | Member removed |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller is not the Captain |
| 404 Not Found | Team or member not found |
| 422 Unprocessable Entity | Attempt to remove the Captain (Captain must transfer captaincy first) |

**FR**: FR-020

---

## POST `/api/teams/{id}/transfer-captaincy`

Transfer captaincy to another team member.

**Auth**: Team Captain only.

**Request body**:
```json
{
  "newCaptainPlayerId": "guid"
}
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Captaincy transferred |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller is not the current Captain |
| 404 Not Found | Team or new Captain Player not found |
| 422 Unprocessable Entity | Target Player is not a member of the team |

**FR**: FR-021

---

## POST `/api/tournaments/{tournamentId}/registrations`

Register a team in a tournament.

**Auth**: Team Captain only.

**Request body**:
```json
{
  "teamId": "guid"
}
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 201 Created | Team registered |
| 400 Bad Request | Validation error |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller is not the Captain of the specified team |
| 404 Not Found | Tournament or team not found |
| 409 Conflict | Team already registered in this tournament |
| 422 Unprocessable Entity | Tournament not Open; videogame mismatch; max slots reached; team has fewer members than `minMembersPerTeam` |

**FR**: FR-022, FR-023

---

## DELETE `/api/tournaments/{tournamentId}/registrations/{teamId}`

Withdraw a team's registration.

**Auth**: Team Captain only.

**Responses**:
| Status | Condition |
|--------|-----------|
| 204 No Content | Registration withdrawn |
| 401 Unauthorized | No token |
| 403 Forbidden | Caller is not the Captain |
| 404 Not Found | Registration not found |
| 422 Unprocessable Entity | Tournament is no longer Open |

**FR**: FR-024
