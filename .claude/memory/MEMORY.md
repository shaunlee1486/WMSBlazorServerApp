# Project Memory — Warehouse Management System (WMS)

## Overview & Technology Stack
- **Core Platform:** .NET 10 (C#) & Blazor Server UI
- **Database Layer:** PostgreSQL
- **Database Migrations:** DbUp (using `dbup-core` and `dbup-postgresql`). It executes scripts from `Scripts`, `SeedData`, and `SampleData` (conditional on `ASPNETCORE_ENVIRONMENT == "Development"`) and tracks them in the default `schemaversions` table.
- **Architecture Pattern:** Clean Architecture (Onion) with MediatR CQRS, FluentValidation pipeline, and Unit of Work behaviors.
- **Identity & Access Management:** ASP.NET Identity Core with custom Identity tables (renamed without `AspNet` prefixes).

---

## Architectural Conventions

### Primary Keys
- All database keys are `UUIDv7` generated via `IIdGenerator` wrapping `Guid.CreateVersion7()`.

### CQRS Pipeline
- **Commands / Queries:** Encapsulated with MediatR.
- **Behaviors:** `ValidationBehavior` -> `LoggingBehavior` -> `UnitOfWorkBehavior` -> `ExceptionBehavior`.
- **Transactions:** `UnitOfWorkBehavior` handles transaction scoping (begins a transaction on command execution, saves changes, commits, and rolls back on exception).

### Database Renaming Map
- Custom mapping applied in `AppDbContext` to strip prefix from standard Identity tables:
  - `AspNetUsers` -> `Users`
  - `AspNetRoles` -> `Roles`
  - `AspNetUserRoles` -> `UserRoles`
  - `AspNetUserClaims` -> `UserClaims`
  - `AspNetRoleClaims` -> `RoleClaims`
  - `AspNetUserLogins` -> `UserLogins`
  - `AspNetUserTokens` -> `UserTokens`

### Soft-Delete & Filtering
- Mapped on all primary entities via `IsDeleted` flag and EF Core global query filters.
- SaveChanges overrides in `AppDbContext` automatically map timestamps and convert hard deletes to soft deletes.

---

## Sprint Roadmap & Progress Status

- **[x] Phase 1 — Solution Scaffold & Infrastructure Foundation**
  - Project structures (`Domain`, `Application`, `Infrastructure`, `Presentation`, `SharedKernel`, `Migration`), identity setup, and Docker Compose configurations.
- **[x] Phase 2 — Master Data Management**
  - CRUD pages and validations for Warehouses, Zones, Locations, Suppliers, Customers, Categories, Units, and Products.
- **[x] Phase 3 — Inventory & Stock Management**
  - Stock level visualization, movements ledger, and stock adjustment approval workflow.
- **[x] Phase 4 — Inbound (Purchase Orders & Goods Receipt)**
  - PO creation/confirmation, Goods Receipt allocation (locations, batch numbers, and expiry dates), and atomic stock increments.
- **[x] Phase 5 — Outbound (Sales Orders, Pick Lists & Goods Issue)**
  - **Sales Orders:** Draft, confirm (evaluates stock globally and reserves quantity at locations using FIFO), and cancel (releases reservations).
  - **Pick Lists:** Generated using FIFO suggestions from reservation locations, picker assignment, checklist updating, and goods issue generation.
  - **Goods Issue:** Dispatches stock, decrements quantity and releases reservation atomically, and registers immutable stock movement records.
- **[x] Phase 6 — Transfer Orders & Returns**
  - **Transfer Orders:** Draft, approve, ship (reserves stock at source location), and receive (decrements source stock, increments destination stock, and registers Transfer stock movements).
  - **Returns Ledger:** Customer and supplier returns. Supplier returns decrement inventory. Customer returns support line-by-line inspection (Accepted/Rejected) and increment target bin stock only for Accepted items upon completion.
- **[x] Phase 7 — Dashboard, Reports & User Management**
  - **Dashboard:** Interactive charts (ApexCharts) for category stock values, inbound vs outbound volume, and top products by transaction, along with a live-updating movements feed.
  - **Reports Portal:** Advanced filters (category, warehouse, date range, user/supplier/customer) and CSV exports for Stock Snapshot, Movement History, Inbound Receipts, Outbound Issues, Stock Adjustments, and granular Audit Logs.
  - **User & Settings:** Administrative creation/editing of users, role assignment permissions (Admin, Manager, Receiver, Picker, etc.), password resets, self-service updates, and global company variable/currency config configurations.
- **[ ] Phase 8 — Hardening, Production Readiness & Deployment** (Upcoming)
