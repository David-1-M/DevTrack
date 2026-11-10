# Project Listing: Pagination and Search

The authenticated endpoint `GET /api/projects` now supports optional query parameters.

| Parameter | Default | Rules |
|---|---|---|
| `page` | `1` | Positive integer |
| `pageSize` | `10` | Integer from 1 to 100 |
| `search` | Not applied | Optional project-name substring, whitespace trimmed, maximum 100 characters |

Example request (requires a valid bearer JWT):

```http
GET /api/projects?page=2&pageSize=5&search=api
Authorization: Bearer <token>
```

Example response shape (illustrative data):

```json
{
  "items": [
    {
      "id": 12,
      "name": "API Improvements",
      "description": "Backend work",
      "createdAt": "2026-11-10T08:00:00Z",
      "taskCount": 3
    }
  ],
  "page": 2,
  "pageSize": 5,
  "totalCount": 9,
  "totalPages": 2
}
```

The API filters projects to the signed-in owner **before** applying optional search, counting matches, or retrieving a page. Results are ordered by project ID to ensure predictable offset paging. Search matching follows the configured database collation (which determines case sensitivity).

Invalid page, page size, or search-length values return HTTP 400. A page beyond the last page returns an empty `items` collection together with accurate total metadata. With no results, `totalPages` is zero.

**Compatibility note:** The previous endpoint returned a bare JSON array. It now returns an object with `items` and pagination metadata. Clients consuming this endpoint must be updated accordingly.

## Test the change locally

Run:

```powershell
dotnet test tests/DevTrack.Api.Tests/DevTrack.Api.Tests.csproj
```

Check these scenarios with two separately authenticated users:

1. Default page returns at most ten projects.
2. Page two contains the next set of projects, without duplicates.
3. Search returns only matching project names belonging to the current user.
4. Both `totalCount` and `totalPages` reflect the filtered owner-scoped dataset.
5. Invalid parameters return HTTP 400.
6. A valid page beyond the available projects contains no items.
7. Neither search nor pagination exposes another user's projects.

These endpoint scenarios should ultimately become automated integration tests; the accompanying unit tests currently validate the query parameter rules.
