# Contract: Videogames

**Base path**: `/api/videogames`
**Auth required**: No (public read-only catalog)

---

## GET `/api/videogames`

List all available videogames from the fixed catalog.

**Auth**: None.

**Response**:
```json
[
  { "id": "guid", "name": "League of Legends" },
  { "id": "guid", "name": "Valorant" },
  { "id": "guid", "name": "CS2" },
  { "id": "guid", "name": "Dota 2" },
  { "id": "guid", "name": "Rocket League" }
]
```

**Responses**:
| Status | Condition |
|--------|-----------|
| 200 OK | Catalog returned |

**Notes**:
- Catalog is fixed at startup via seed data. No runtime additions, edits, or removals.
- This endpoint exists to populate dropdowns during Player registration and Team/Tournament creation.

**FR**: Videogame catalog assumption (spec section "Assumptions")
