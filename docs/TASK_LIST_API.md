# Task Listing API

The authenticated endpoint `GET /api/tasks` lists tasks belonging to projects owned by the current user.

## Query options

| Parameter | Default | Accepted values |
|---|---|---|
| `status` | All | `ToDo`, `InProgress`, `Completed` (case-insensitive) |
| `projectId` | All owned projects | Positive integer |
| `sortBy` | `createdAt` | `createdAt` or `dueDate` |
| `sortOrder` | `asc` | `asc` or `desc` |
| `page` | `1` | Positive integer |
| `pageSize` | `10` | 1–100 |

Example:

```http
GET /api/tasks?status=InProgress&projectId=2&sortBy=dueDate&sortOrder=asc&page=1&pageSize=10
Authorization: Bearer <token>
```

Response shape (sample data):

```json
{
  "items": [
    {
      "id": 8,
      "title": "Add filtering",
      "description": null,
      "status": "InProgress",
      "createdAt": "2026-11-11T08:00:00Z",
      "dueDate": "2026-11-20T15:00:00Z",
      "projectId": 2
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

Filtering by status and project occurs **after ownership restrictions** and before counting/pagination. An inaccessible project ID reveals no tasks. Invalid query settings return HTTP 400. Results use a stable task ID tie-breaker. For due-date ordering, tasks without due dates appear last in either direction.

## Verification checklist

- Run `dotnet test tests/DevTrack.Api.Tests/DevTrack.Api.Tests.csproj`.
- Confirm ToDo, InProgress and Completed filters.
- Compare due-date ascending/descending ordering, including null due dates.
- Verify default and custom pagination and accurate counts.
- Verify invalid filters return HTTP 400.
- Test with two users: no task from one user's project should appear for the other user.

The accompanying unit tests validate query parsing. HTTP integration tests and manual checks remain to be completed.
