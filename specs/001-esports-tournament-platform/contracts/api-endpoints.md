# API Endpoints Contract

**Base URL**: `/api`
**Authentication**: JWT Bearer token in Authorization header
**Content-Type**: `application/json`

## Authentication Endpoints

### POST /api/auth/register/organizer
**Description**: Register new organizer account
**Auth**: None
**Request Body**:
```json
{
  "organizationName": "string (3-60 chars, unique)",
  "email": "string (valid email, unique)",
  "password": "string (8+ chars, 1 upper, 1 lower, 1 number)"
}
```
**Responses**:
- `201 Created`: Account created, returns token
- `400 Bad Request`: Validation errors
- `409 Conflict`: Email/organization name already exists

### POST /api/auth/register/player
**Description**: Register new player account
**Auth**: None
**Request Body**:
```json
{
  "username": "string (unique)",
  "realName": "string",
  "email": "string (valid email, unique)",
  "password": "string (8+ chars, 1 upper, 1 lower, 1 number)",
  "mainGame": "string"
}
```
**Responses**: Same as organizer registration

### POST /api/auth/login
**Description**: Login for all user types
**Auth**: None
**Request Body**:
```json
{
  "email": "string",
  "password": "string"
}
```
**Responses**:
- `200 OK`: Returns JWT token + user info
- `401 Unauthorized`: Invalid credentials

---

## Tournament Endpoints

### POST /api/tournaments
**Description**: Create new tournament
**Auth**: Organizer role required
**Request Body**:
```json
{
  "name": "string",
  "game": "string",
  "description": "string (optional)",
  "startDate": "datetime (future)",
  "endDate": "datetime (after startDate)",
  "maxTeams": "int (min 2)",
  "minPlayersPerTeam": "int (default 5)",
  "scoringSystemType": "Standard | WTA | Custom",
  "customWinPoints": "int (if Custom)",
  "customDrawPoints": "int (if Custom)",
  "customLossPoints": "int (if Custom)"
}
```
**Responses**:
- `201 Created`: Tournament created
- `400 Bad Request`: Validation errors
- `401/403`: Auth errors

### GET /api/tournaments
**Description**: List all visible tournaments (draft only for owner, others for all)
**Auth**: Required
**Query Params**: `?state=Open&game=Valorant`
**Responses**:
- `200 OK`: Array of tournaments

### GET /api/tournaments/{id}
**Description**: Get tournament details
**Auth**: Required (draft only if owner)
**Responses**:
- `200 OK`: Tournament object
- `403 Forbidden`: Draft tournament, not owner
- `404 Not Found`

### PUT /api/tournaments/{id}
**Description**: Update tournament (only own tournaments)
**Auth**: Organizer role required
**Request Body**: Same as POST
**Responses**:
- `200 OK`: Updated
- `400 Bad Request`: Validation or immutable field (scoring after InProgress)
- `403 Forbidden`: Not owner

### PUT /api/tournaments/{id}/state
**Description**: Transition tournament state
**Auth**: Organizer role required
**Request Body**:
```json
{
  "newState": "Open | InProgress | Finished"
}
```
**Responses**:
- `200 OK`: State changed
- `400 Bad Request`: Invalid transition (backward)
- `403 Forbidden`: Not owner

---

## Team Endpoints

### POST /api/teams
**Description**: Create new team
**Auth**: Player role required
**Request Body**:
```json
{
  "name": "string (unique within game)",
  "description": "string (optional)",
  "game": "string"
}
```
**Responses**:
- `201 Created`: Team created, captain = current user
- `400 Bad Request`: Already captain of team for this game
- `409 Conflict`: Name taken for game

### GET /api/teams/{id}
**Description**: Get team details
**Auth**: Required
**Responses**:
- `200 OK`: Team with members list

### POST /api/teams/{id}/invite
**Description**: Invite player to team
**Auth**: Must be team captain
**Request Body**:
```json
{
  "playerIdentifier": "string (email or username)"
}
```
**Responses**:
- `201 Created`: Invitation sent
- `400 Bad Request`: Player not found or already in team for game
- `403 Forbidden`: Not captain

### POST /api/teams/{id}/invitations/{invitationId}/respond
**Description**: Accept/reject invitation
**Auth**: Must be invited player
**Request Body**:
```json
{
  "accept": "boolean"
}
```
**Responses**:
- `200 OK`: Responded
- `400 Bad Request`: Already in team for game (if accepting)
- `403 Forbidden`: Not the invited player

### DELETE /api/teams/{id}/members/{playerId}
**Description**: Remove player from team
**Auth**: Must be team captain
**Responses**:
- `204 No Content`: Removed
- `403 Forbidden`: Not captain

---

## Tournament Enrollment Endpoints

### POST /api/tournaments/{id}/enroll
**Description**: Enroll team in tournament
**Auth**: Must be team captain
**Request Body**:
```json
{
  "teamId": "guid"
}
```
**Responses**:
- `201 Created`: Enrolled
- `400 Bad Request`: Validation failed (game mismatch, insufficient players, tournament full, already enrolled)
- `403 Forbidden`: Not captain or tournament not Open

### GET /api/tournaments/{id}/teams
**Description**: List enrolled teams
**Auth**: Required
**Responses**:
- `200 OK`: Array of teams

---

## Match Endpoints

### POST /api/tournaments/{id}/matches
**Description**: Register match result
**Auth**: Must be tournament organizer
**Request Body**:
```json
{
  "homeTeamId": "guid",
  "awayTeamId": "guid",
  "homeScore": "int",
  "awayScore": "int",
  "matchDate": "datetime"
}
```
**Responses**:
- `201 Created`: Result saved, standings recalculated
- `400 Bad Request`: Teams not enrolled, same team twice, duplicate match
- `403 Forbidden`: Not tournament organizer

### GET /api/tournaments/{id}/matches
**Description**: List tournament matches
**Auth**: Required
**Responses**:
- `200 OK`: Array of matches

---

## Standings Endpoints

### GET /api/tournaments/{id}/standings
**Description**: Get tournament standings (InProgress or Finished only)
**Auth**: Required
**Responses**:
- `200 OK`: Array of standings (position, team, points, played, won, drawn, lost)
- `400 Bad Request`: Tournament not InProgress or Finished

---

## User Profile Endpoints

### GET /api/users/me
**Description**: Get current user profile
**Auth**: Required
**Responses**:
- `200 OK`: User object with role-specific fields

### PUT /api/users/me
**Description**: Update current user profile
**Auth**: Required
**Request Body** (Organizer):
```json
{
  "organizationName": "string (optional)",
  "password": "string (optional)"
}
```
**Request Body** (Player):
```json
{
  "realName": "string (optional)",
  "email": "string (optional)",
  "password": "string (optional)"
}
```
**Responses**:
- `200 OK`: Updated
- `400 Bad Request`: Validation errors

---

## Error Response Format

All error responses follow this structure:
```json
{
  "error": "Mensaje descriptivo del error",
  "details": ["Validation error 1", "Validation error 2"]
}
```

Status codes:
- `400`: Validation / business rule violation
- `401`: Not authenticated
- `403`: Authenticated but lacking permission
- `404`: Resource not found
- `409`: Conflict (duplicate)
- `500`: Server error (no internal details exposed)
