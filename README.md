# DevTrack API

**A secure RESTful project and task management API built with ASP.NET Core, Entity Framework Core, and SQL Server.**

## Overview

DevTrack API is a backend application designed to help users organise projects and manage development tasks. It demonstrates RESTful API development, relational database design, authentication, authorization, and automated testing using modern .NET technologies.

## Technologies Used

- **Backend:** C#, ASP.NET Core Minimal APIs, .NET 9
- **Database:** SQL Server, Entity Framework Core
- **Authentication:** ASP.NET Core Identity, JWT Bearer Authentication
- **Security:** Role-Based Authorization, Resource Ownership
- **Testing:** xUnit
- **CI/CD:** GitHub Actions
- **Development Tools:** Visual Studio 2026, Git, GitHub

## Features

### Project Management
- Create, retrieve, update, and delete projects.
- View tasks associated with a project.
- Prevent deletion of projects containing tasks.

### Task Management
- Create, retrieve, update, and delete tasks.
- Associate tasks with projects.
- Track task status: To Do, In Progress, Completed.

### Authentication & Security
- User registration and login.
- Secure password hashing through ASP.NET Core Identity.
- JWT-based authentication.
- User and Admin roles.
- Ownership-based access control for projects and tasks.
- Protected API endpoints.

### Automated Testing
- xUnit validation tests.
- Automated build and testing through GitHub Actions.
- Manually verified authentication, authorization, and cross-user access restrictions.

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/health` | Check API health |
| POST | `/api/auth/register` | Register a user |
| POST | `/api/auth/login` | Log in |
| GET | `/api/auth/me` | Retrieve authenticated user |
| GET | `/api/projects` | Retrieve owned projects |
| GET | `/api/projects/{id}` | Retrieve a project |
| POST | `/api/projects` | Create a project |
| PUT | `/api/projects/{id}` | Update a project |
| DELETE | `/api/projects/{id}` | Delete a project |
| GET | `/api/projects/{id}/tasks` | Retrieve project tasks |
| GET | `/api/tasks/{id}` | Retrieve a task |
| POST | `/api/tasks` | Create a task |
| PUT | `/api/tasks/{id}` | Update a task |
| DELETE | `/api/tasks/{id}` | Delete a task |
| GET | `/api/admin/status` | Administrator-only test endpoint |

## Running Locally

### Prerequisites
- Compatible .NET 9 SDK
- SQL Server LocalDB or SQL Server
- Git

### Setup

Clone the repository and navigate into the API project:

```bash
git clone https://github.com/David-1-M/DevTrack.git
cd DevTrack/backend/DevTrack.Api
```

Configure the `DefaultConnection` SQL Server connection string.

Configure the JWT signing key using .NET User Secrets. Never commit signing keys or database credentials.

Apply the existing EF Core migrations:

```bash
dotnet ef database update
```

Run the application:

```bash
dotnet run
```

Use the localhost address printed in the terminal to access the API.

## Testing

Run the test suite from the repository root:

```bash
dotnet test tests/DevTrack.Api.Tests/DevTrack.Api.Tests.csproj
```

**Latest verified result:** 7 tests passed, 0 failed.

GitHub Actions automatically builds and tests the project when changes are pushed to the configured branch or a pull request is opened against it.

## Architecture

DevTrack follows a modular ASP.NET Core Minimal API structure.

- `Endpoints/` — HTTP route handlers
- `Models/` — Domain entities and Identity user
- `DTOs/` — Request data contracts
- `Data/` — Entity Framework Core database context
- `Services/` — JWT generation
- `Validation/` — Request validation logic
- `Migrations/` — Database schema migrations
- `tests/` — Automated tests

## Security and Limitations

DevTrack is a locally tested portfolio application, not a production-ready identity platform.

Its JWT implementation is intended for development and educational purposes. Production deployment would require additional security hardening, including standards-based token issuance, account recovery, email verification, rate limiting, secure key management and further integration testing.

Cloud hosting and container deployment have not yet been implemented.

## Future Improvements

- Docker containerisation
- Microsoft Azure deployment
- Additional API integration and security regression tests
- Production-grade authentication infrastructure

## Author

**David Mdluli**

BSc Information Technology — North-West University

Software Engineering | Backend Development | Artificial Intelligence