# Walkthrough - WMS Phase 1 Implementation Complete

We have completed the implementation of **Phase 1 — Solution Scaffold & Infrastructure Foundation** for the Warehouse Management System (WMS). The solution compiles warning-free under .NET 10.0.

Here is a summary of what has been implemented and how to run/verify it.

---

## 1. Project Scaffolding (Clean Architecture / Onion)

We initialized the `WMS.sln` solution and scaffolded the following projects, configuring their references to follow Onion Architecture principles:

- **[WMS.SharedKernel](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.SharedKernel)**: Holds core base classes, result structures, Guid generators, and domain enums.
- **[WMS.Domain](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Domain)**: Contains identity entity mappings (`AppUser`, `AppRole`) and data access contracts (`IRepository`, `IUnitOfWork`).
- **[WMS.Application](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Application)**: CQRS pipelines, logging/validation middleware behaviors.
- **[WMS.Infrastructure](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Infrastructure)**: Database context (`AppDbContext`), generic `EFRepository` implementation, identity, and storage services.
- **[WMS.Presentation](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation)**: Blazor Server Web App providing interactive UI layouts, authentication guards, and API endpoints.
- **[WMS.Migration](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration)**: Custom database migration console runner.

---

## 2. Infrastructure & Application Pipeline

We set up advanced pipeline behaviors using **MediatR** to wrap all CQRS command and query handlers:

1. **[LoggingBehavior.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Application/Common/Behaviors/LoggingBehavior.cs)**: Tracks execution duration and logs request statistics.
2. **[ValidationBehavior.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Application/Common/Behaviors/ValidationBehavior.cs)**: Automatically scans for and executes FluentValidation rules.
3. **[UnitOfWorkBehavior.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Application/Common/Behaviors/UnitOfWorkBehavior.cs)**: Automatically wraps all command operations in database transactions.
4. **[ExceptionBehavior.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Application/Common/Behaviors/ExceptionBehavior.cs)**: Catches unhandled errors and converts them to failed `Result` instances.

---

## 3. Database Schema & Custom Migration Runner

Instead of using the legacy `roundhouse` tool, which can have dependency conflicts under .NET 10, we implemented a custom PostgreSQL script runner in **WMS.Migration** using `Npgsql`.
- It connects to PostgreSQL, creates the target database if missing, and runs SQL files in order (`Scripts` -> `SeedData` -> `SampleData`).
- All scripts are idempotent (`CREATE TABLE IF NOT EXISTS`, `ON CONFLICT DO UPDATE/NOTHING`).

The generated SQL scripts include:
- **[0001_create_identity_tables.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/Scripts/0001_create_identity_tables.sql)**: Renamed Identity tables (`Users`, `Roles`, `UserRoles`, etc.).
- **[0002_create_master_data_tables.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/Scripts/0002_create_master_data_tables.sql)**: Warehouses, Zones, Locations, Suppliers, Customers, Categories, Units, Products.
- **[0003_create_inventory_tables.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/Scripts/0003_create_inventory_tables.sql)**: Stocks and Stock Movements audit ledger.
- **[0004_create_inbound_tables.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/Scripts/0004_create_inbound_tables.sql)**: Purchase Orders and Goods Receipts.
- **[0005_create_outbound_tables.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/Scripts/0005_create_outbound_tables.sql)**: Sales Orders, Pick Lists, and Goods Issues.
- **[0006_create_internal_tables.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/Scripts/0006_create_internal_tables.sql)**: Transfer Orders, Returns, Audit Logs, and App Settings.
- **[0001_seed_roles.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/SeedData/0001_seed_roles.sql)**: Admin, WarehouseManager, Receiver, Picker, StockController, Viewer.
- **[0002_seed_admin_user.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/SeedData/0002_seed_admin_user.sql)**: Seeding user `admin` with PBKDF2 SHA256 hashed password `Admin123!`.
- **[0001_sample_warehouses.sql](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Migration/SampleData/0001_sample_warehouses.sql)**: Sample warehouses (WH-001, WH-002) with storage locations.
- **Other sample scripts**: Products (`Smart Hub Pro`, `Steel Bracket`), Suppliers, Customers, and initial stock quantities.

---

## 4. Presentation (Blazor Server & Cookie Auth)

We configured cookie-based ASP.NET Identity in `Program.cs` and routed login operations through secure endpoints:

- **[Login.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Auth/Login.razor)**: Premium styled glassmorphic sign-in panel. Input forms are pre-populated with the seeded `admin` credentials for easy testing.
- **[Logout.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Auth/Logout.razor)**: Clears active cookies and signs out user.
- **[MainLayout.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Layout/MainLayout.razor)**: Beautiful layout containing sidebar, interactive top header displaying active profile name & roles.
- **[Home.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Home.razor)**: Dashboard page containing summary metric cards and active nodes indicators.

---

## 5. Dockerization & Deployment Structure

We added configuration files to run the entire system in containers:
- **[docker-compose.yml](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/docker-compose.yml)**: Sets up `db` (Postgres 16), `migrate` (runs migration), and `app` (Blazor app running on port 5000) services.
- **[Dockerfile.app](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/Dockerfile.app)**: Multi-stage build for Presentation.
- **[Dockerfile.migration](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/Dockerfile.migration)**: Multi-stage build for custom migration script runner.

---

## How to Verify

### Step 1: Recompile Solution
To check compilation validity, you can run:
```powershell
dotnet build
```

### Step 2: Spin Up Containers
Once you have started your local Docker Desktop/daemon, you can spin up the whole system:
```powershell
docker-compose up --build
```
This will:
1. Initialize the PostgreSQL 16 server.
2. Build and run the `migrate` container to create tables, seed roles, and insert sample data.
3. Start the Blazor presentation web application on `http://localhost:5000`.

### Step 3: Browse WMS
Navigate to `http://localhost:5000` in your web browser. You will be redirected to the sign-in page. Click **Sign In** using the pre-loaded credentials (`admin` / `Admin123!`) to access the dashboard.
