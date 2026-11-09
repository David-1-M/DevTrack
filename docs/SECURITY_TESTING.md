# API Security Regression Checklist

Use this checklist after changes to authentication, authorization, project ownership, or task endpoints.

> This is a manual regression guide for local development, not proof of production security. Never commit real JWTs, passwords, or signing keys.

## Before testing

1. Run the API using a development database with disposable data.
2. Register two non-admin test users, **User A** and **User B**.
3. Log in as both users and save each JWT only in your local API client.
4. Create a project and task belonging to User A; record their IDs.
5. Create a separate project belonging to User B.

## Authentication checks

| Scenario | Expected result |
|---|---|
| Request a protected endpoint without a token | HTTP 401 |
| Request a protected endpoint with a malformed token | HTTP 401 |
| Request a protected endpoint with an expired token | HTTP 401 |
| Log in with an incorrect password | Login rejected |
| Access `/api/auth/me` with a valid token | Only the authenticated user's identity is returned |
| Access `/api/admin/status` with a non-admin token | HTTP 403 |

## Ownership checks

With **User B's token**, attempt the following operations against **User A's** resources:

- Retrieve A's project via `GET /api/projects/{id}`.
- Update A's project via `PUT /api/projects/{id}`.
- Delete A's project via `DELETE /api/projects/{id}`.
- List A's project tasks via `GET /api/projects/{id}/tasks`.
- Retrieve, update, and delete A's task via the corresponding `/api/tasks/{id}` routes.
- Attempt to create a task under A's project.

**Expected:** Every cross-user operation must be denied without disclosing private resource contents or mutating A's data. Depending on endpoint design, inaccessible resources may produce HTTP 403 or 404. Verify the database state after each attempted write.

## Input and data integrity checks

- Submit missing or invalid required fields; check that validation rejects them.
- Attempt to create a task for a nonexistent project.
- Attempt to delete a project that still contains tasks; verify the documented restriction.
- Check that ordinary users cannot assign themselves an administrator role through registration or profile input.
- Verify that error responses do not expose connection strings, JWT signing keys, or stack traces.

## Automated tests and evidence

From the repository root:

```powershell
dotnet test tests/DevTrack.Api.Tests/DevTrack.Api.Tests.csproj
```

Record the test date, commit SHA, and pass/fail result when running this checklist. Report failures as GitHub issues with sanitized request/response examples; **never** include credentials or live tokens.

## Production considerations

This portfolio API is not described as production hardened. Before public deployment, assess rate limiting, HTTPS enforcement, account verification and recovery, secret management, token issuance/rotation, audit logging, and automated authorization integration tests.
