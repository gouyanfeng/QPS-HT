# QPS Backend

QPS Backend is an ASP.NET Core 8 service that now keeps only the system
administration surface: authentication, users, roles, permissions, data
dictionaries, and error logging.

## Tech Stack

| Category | Technology |
| --- | --- |
| Framework | ASP.NET Core 8 |
| Language | C# |
| ORM | Entity Framework Core 8 |
| Database | SQLite |
| CQRS | MediatR |
| Auth | JWT |
| Validation | FluentValidation |
| API Docs | Swagger |

## Project Layout

```text
src/
  1.QPS.Domain/          Domain entities and shared primitives
  2.QPS.Application/     Application commands, queries, DTOs, and interfaces
  3.QPS.Infrastructure/  EF Core, identity, and service implementations
  4.QPS.WebAPI/          API controllers, filters, and startup
tests/
  QPS.UnitTests/
  QPS.IntegrationTests/
```

## API Areas

| Controller | Route | Purpose |
| --- | --- | --- |
| AuthController | /api/auth | Login/logout |
| RoleController | /api/admin/roles | Role management |
| UserController | /api/admin/users | User management |
| PermissionController | /api/admin/permissions | Permission tree and role permissions |
| DataDictionaryController | /api/admin/data-dictionaries | Data dictionary management |

## Run

```bash
dotnet restore QPS.sln
dotnet build QPS.sln
dotnet run --project src/4.QPS.WebAPI/QPS.WebAPI.csproj
```

Swagger is available at `/swagger` when the API is running.
