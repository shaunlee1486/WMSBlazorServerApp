# Warehouse Management System (WMS)

A production-grade, highly performant Warehouse Management System (WMS) built with **.NET 10 Blazor Server**, **Entity Framework Core**, **PostgreSQL**, and **Clean Architecture**.

---

## Architecture Overview

The codebase is structured according to Clean Architecture principles:
- **WMS.Domain**: Core domain entities (Warehouses, Products, Stocks, Movements, Adjustments, POs, SOs, Pick Lists, Goods Issues, Transfers, Returns).
- **WMS.Application**: MediatR CQRS pipeline, request validators (FluentValidation), and interfaces.
- **WMS.Infrastructure**: Persistence (EF Core, database mappings, Npgsql, repositories), decoupled Identity (ASP.NET Identity mapping to custom tables), and logging/UoW pipeline behaviors.
- **WMS.Presentation**: Blazor Server UI (KPI cards, interactive ApexCharts, reports, user ledger, audit trail diff viewer, status controls).
- **WMS.Migration**: DbUp-based automatic database schema migration and seed runner.

---

## Key Features

- **Inventory Tracking**: Stock counts, bin locations, batch tracking, expiry dates, and atomic stock transfers.
- **Fulfillment Workflows**: FIFO picking suggestions, goods receipts/issues, transfer orders, and returns with line-level inspections.
- **Analytics & Dashboard**: Interactive visual widgets displaying category values, timeline volume, and live movement feeds.
- **Auditable Records**: Granular change logs capturing field-level before/after JSON diffs.
- **Security**: Strict role-based page authorization, password complexity policies, and account lockout.

---

## Prerequisites

Ensure you have the following installed:
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for containerized hosting)
- [PostgreSQL](https://www.postgresql.org/) (if running database locally)

---

## Setup & Running the Application

### Method 1: Containerized Hosting (Docker & Docker Compose)

The repository provides automated Docker orchestration through a multi-container `docker-compose.yml` configuration:
- **wms_postgres_db**: PostgreSQL 16 database storage.
- **wms_migration_runner**: Self-terminating container that runs DbUp schema scripts and default seeds.
- **wms_blazor_app**: The ASP.NET Core Blazor Server application.

#### 1. Quick Start
Build and run the entire stack in the background:
```powershell
docker-compose up -d --build
```
The Blazor WMS interface will be accessible at: `http://localhost:5000`

#### 2. Monitoring Startup Logs
Check the status of database seeding and build compilation:
```powershell
# View logs for all services
docker-compose logs -f

# View logs specifically for migration runner
docker-compose logs -f migrate
```

#### 3. Re-seeding / Resetting the Database
If you need to drop the PostgreSQL tables and run seed data from scratch:
```powershell
# Stop services and destroy postgres data volumes
docker-compose down -v

# Start services and trigger clean seeding
docker-compose up -d --build
```

#### 4. Building Services Individually
You can compile and build the Docker images for each service independently:
```powershell
# Build only the Blazor web application image
docker-compose build app

# Build only the migration runner image
docker-compose build migrate

# Build only the PostgreSQL database image
docker-compose build db
```

> [!NOTE]
> **Difference between commands**:
> - `docker-compose build app`: Only compiles and bakes the new Docker image; it **does not** start, stop, or restart the container.
> - `docker-compose up -d --build app`: Rebuilds the image **and** immediately restarts/updates (Yes (replaces running app container)) the running container in the background, also triggering any dependent services (e.g., database, migration runner). Trigger Dependencies? Yes (starts database and migration runner)


### Method 2: Running Locally
1. Start a local PostgreSQL server on port `5432` with username `wms_user` and password `wms_pass` (or update `appsettings.json` connection string).
2. Run database migrations:
   ```powershell
   dotnet run --project WMS.Migration/WMS.Migration.csproj
   ```
3. Run the Blazor application:
   ```powershell
   dotnet run --project WMS.Presentation/WMS.Presentation.csproj
   ```
   Open `http://localhost:5005` in your browser.

---

## Seed Credentials

On first run, the database is seeded with a default system administrator account:
- **Username**: `admin`
- **Password**: `Admin123!`
- **Roles**: `Admin`

---

## Production Configurations

### 1. Reverse Proxy & SignalR
When hosting behind Nginx, use the reference configurations in `nginx.conf`. Ensure WebSockets are enabled and proxy headers are forwarded:
- `Connection: Upgrade`
- `Upgrade: $http_upgrade`
- `X-Forwarded-For` & `X-Forwarded-Proto` (handled in Blazor by `UseForwardedHeaders` middleware)

### 2. Health Checks
The app exposes an automated health status check endpoint at `/health` returning HTTP 200 OK (`Healthy`) under normal operating parameters.
