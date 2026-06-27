# CLAUDE.md — Developer Guidelines

This file outlines build, run, and development guidelines for the Warehouse Management System (WMS) application.

## Build and Run Commands

### 1. Build Solution
Verify that all projects compile with zero warnings:
```powershell
dotnet build WMS.sln
```

### 2. Run Local Development Server
Launch the Blazor Presentation project:
```powershell
dotnet run --project WMS.Presentation/WMS.Presentation.csproj
```
- Local HTTP URL: `http://localhost:5005`
- Local HTTPS URL: `https://localhost:7236`

### 3. Run Database Migrations
Execute DB schema updates and seed files locally:
```powershell
dotnet run --project WMS.Migration/WMS.Migration.csproj
```

### 4. Docker Production Compose
Build, migrate, and host the PostgreSQL database, migration runner, and Blazor application containerized:
```powershell
# Start services
docker-compose up -d --build

# Stop services and clear Postgres volume
docker-compose down -v
```

### 5. Health Checks
Validate API/App health status:
- URL: `http://localhost:5005/health` (returns `Healthy`)

---

## Code Style & Architectural Conventions

### 1. Clean Architecture Layers
- **WMS.Domain**: Pure enterprise entities, value objects, and repository interfaces. No external dependencies.
- **WMS.Application**: CQRS Handlers (Queries/Commands), DTOs, pipeline behaviors, and FluentValidation rules.
- **WMS.Infrastructure**: EF Core DbContext, repository implementations, IdentityService, and third-party integrations.
- **WMS.Presentation**: Blazor Server components, CSS variables, routes, and controllers.

### 2. CQRS Use Cases
- Implement all core functionality via MediatR Commands/Queries.
- Place feature classes in `WMS.Application/Features/<FeatureName>/<Commands|Queries|DTOs>/`.
- Define validators using `AbstractValidator<TRequest>` alongside Commands/Queries.

### 3. C# Styling & Idioms
- Use **PascalCase** for public members, classes, and namespaces.
- Use **camelCase** for local variables and parameters.
- Prefix private fields with an underscore (e.g., `_currentUserService`).
- Enforce clean null-safety. Use nullable annotations (`?`) on optional fields and null-forgiving operators (`!`) to satisfy compiler analysis on required database navigation properties.
- Enable `AsNoTracking()` for all read-only LINQ query projection handlers.
