# Warehouse Management System — Implementation Plan

**Stack:** .NET 10 · Blazor Server · ASP.NET Identity Core · PostgreSQL · Roundhouse · MediatR · FluentValidation · Clean Architecture (Onion)

---

## Architecture Overview

```
WMS.sln
├── WMS.Domain              # Entities, value objects, domain events, interfaces
├── WMS.Application         # CQRS (Commands/Queries/Handlers), DTOs, Validators, Interfaces
├── WMS.Infrastructure      # EF Core DbContext, Repositories, Identity, Services
├── WMS.Presentation        # Blazor Server UI (Pages, Components, Layout)
├── WMS.Migration           # Roundhouse console project (Scripts / SeedData / SampleData)
└── WMS.SharedKernel        # BaseEntity, Result<T>, IIdGenerator, Pagination, Enums
```

### Key Conventions

| Rule | Detail |
|---|---|
| Primary Keys | UUIDv7 via `IIdGenerator` wrapping `Guid.CreateVersion7()` |
| CQRS | Every use-case = Command **or** Query + Handler + Validator + DTO |
| Pipeline | ValidationBehavior → LoggingBehavior → UnitOfWorkBehavior → ExceptionBehavior |
| Migrations | Roundhouse owns **all** schema; EF Core = query/mapping only (no `Migrate()`, no `EnsureCreated()`) |
| Identity tables | Prefix stripped: `AspNetUsers` → `Users`, `AspNetRoles` → `Roles`, etc. |
| DI | Constructor injection only; no static classes; `DbContext` never exposed outside Infrastructure |
| Auth | Cookie-based via ASP.NET Identity; role + claim guards on components |
| File storage | Named Docker volume for `wwwroot/uploads`; only relative paths persisted in DB |

---

## Database — Identity Table Rename Map

| ASP.NET Identity Default | Renamed Table |
|---|---|
| `AspNetUsers` | `Users` |
| `AspNetRoles` | `Roles` |
| `AspNetUserRoles` | `UserRoles` |
| `AspNetUserClaims` | `UserClaims` |
| `AspNetRoleClaims` | `RoleClaims` |
| `AspNetUserLogins` | `UserLogins` |
| `AspNetUserTokens` | `UserTokens` |

Achieved via `OnModelCreating` overrides in `AppDbContext`:

```csharp
builder.Entity<AppUser>().ToTable("Users");
builder.Entity<AppRole>().ToTable("Roles");
// ... etc.
```

---

## Domain Model — Core Entities

### Identity & Access
- `AppUser` (extends `IdentityUser<Guid>`) — FullName, AvatarPath, IsActive, CreatedAt
- `AppRole` (extends `IdentityRole<Guid>`) — Description

### Master Data
- `Warehouse` — Code, Name, Address, IsActive
- `Zone` — WarehouseId, Code, Name, ZoneType (Receiving/Storage/Staging/Shipping)
- `Location` — ZoneId, Aisle, Bay, Level, Position, Barcode, IsActive, MaxCapacity
- `Supplier` — Code, Name, ContactPerson, Phone, Email, Address, IsActive
- `Customer` — Code, Name, ContactPerson, Phone, Email, Address, IsActive
- `Category` — Code, Name, ParentCategoryId (self-referencing tree)
- `Unit` — Code, Name (pcs, box, pallet, kg, …)
- `Product` — Code, Name, CategoryId, UnitId, SupplierId, Barcode, Description, ImagePath, MinStock, MaxStock, ReorderPoint, IsActive

### Inventory
- `Stock` — ProductId, LocationId, Quantity, ReservedQuantity, LastUpdatedAt
- `StockMovement` — ProductId, FromLocationId, ToLocationId, Quantity, MovementType, ReferenceNo, Note, CreatedBy, CreatedAt

### Inbound (Receiving)
- `PurchaseOrder` — PONumber, SupplierId, Status, OrderDate, ExpectedDate, Note, CreatedBy
- `PurchaseOrderItem` — PurchaseOrderId, ProductId, OrderedQty, ReceivedQty, UnitPrice
- `GoodsReceipt` — GRNumber, PurchaseOrderId, ReceivedDate, ReceivedBy, Status, Note
- `GoodsReceiptItem` — GoodsReceiptId, ProductId, LocationId, ReceivedQty, BatchNo, ExpiryDate

### Outbound (Shipping)
- `SalesOrder` — SONumber, CustomerId, Status, OrderDate, RequiredDate, Note, CreatedBy
- `SalesOrderItem` — SalesOrderId, ProductId, OrderedQty, PickedQty, UnitPrice
- `PickList` — PickListNumber, SalesOrderId, AssignedTo, Status, CreatedAt
- `PickListItem` — PickListId, ProductId, LocationId, RequiredQty, PickedQty, Status
- `GoodsIssue` — GINumber, SalesOrderId, IssuedDate, IssuedBy, Status, Note
- `GoodsIssueItem` — GoodsIssueId, ProductId, LocationId, IssuedQty, BatchNo

### Internal
- `TransferOrder` — TONumber, FromWarehouseId, ToWarehouseId, Status, RequestedBy, ApprovedBy
- `TransferOrderItem` — TransferOrderId, ProductId, FromLocationId, ToLocationId, Qty, Status
- `StockAdjustment` — AdjNumber, WarehouseId, AdjustmentDate, Reason, ApprovedBy, Status
- `StockAdjustmentItem` — StockAdjustmentId, ProductId, LocationId, SystemQty, ActualQty, Difference

### Reporting / Config
- `AuditLog` — EntityName, EntityId, Action, OldValues (JSON), NewValues (JSON), UserId, Timestamp
- `AppSetting` — Key, Value, Description, Group

---

## Enumerations (in WMS.SharedKernel)

```
PurchaseOrderStatus  : Draft | Confirmed | PartialReceived | FullyReceived | Cancelled
GoodsReceiptStatus   : Draft | Completed | Cancelled
SalesOrderStatus     : Draft | Confirmed | PartialPicked | FullyPicked | Shipped | Cancelled
PickListStatus       : Pending | InProgress | Completed | Cancelled
GoodsIssueStatus     : Draft | Completed | Cancelled
TransferOrderStatus  : Draft | Approved | InTransit | Completed | Cancelled
AdjustmentStatus     : Draft | PendingApproval | Approved | Rejected
MovementType         : Receipt | Issue | Transfer | Adjustment | Return
ZoneType             : Receiving | Storage | Staging | Shipping | Return
```

---

## Roles & Permissions

| Role | Capabilities |
|---|---|
| `Admin` | Full system access, user management, settings |
| `WarehouseManager` | All warehouse ops, approve adjustments/transfers, reports |
| `Receiver` | Create/complete GoodsReceipts, view POs |
| `Picker` | View/work PickLists, create GoodsIssues |
| `StockController` | Stock adjustments, cycle counts, stock reports |
| `Viewer` | Read-only dashboard & reports |

---

## Sprint Roadmap

---

### Phase 1 — Solution Scaffold & Infrastructure Foundation
**Goal:** Runnable skeleton with Identity, DB, Roundhouse pipeline, Docker, and CI-ready structure.

#### 1.1 Solution & Project Setup
- `dotnet new sln -n WMS`
- Create projects: `WMS.Domain`, `WMS.Application`, `WMS.Infrastructure`, `WMS.Presentation`, `WMS.Migration`, `WMS.SharedKernel`
- Add project references following Onion dependency direction:
  ```
  Presentation → Application → Domain
  Infrastructure → Application
  Infrastructure → SharedKernel
  Application → SharedKernel
  ```
- Install NuGet packages:
  - All projects: `Microsoft.Extensions.DependencyInjection.Abstractions`
  - Application: `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`
  - Infrastructure: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - Presentation: `Microsoft.AspNetCore.Components.Server` (Blazor Server)
  - Migration: `roundhouse` (CLI tool via `dotnet-roundhouse`)

#### 1.2 SharedKernel
```
WMS.SharedKernel/
├── BaseEntity.cs          # Id (Guid), CreatedAt, UpdatedAt, IsDeleted
├── AuditableEntity.cs     # extends BaseEntity + CreatedBy, UpdatedBy
├── Result.cs              # Result<T>, Result.Ok(), Result.Fail()
├── IIdGenerator.cs        # Guid Generate()
├── UuidV7Generator.cs     # Guid.CreateVersion7()
├── PagedResult.cs         # Items, TotalCount, Page, PageSize
├── Enums/                 # All domain enums
└── Extensions/            # StringExtensions, EnumExtensions
```

#### 1.3 Domain Layer
```
WMS.Domain/
├── Entities/              # All domain entities (plain POCO, no EF attributes)
│   ├── Identity/
│   │   ├── AppUser.cs
│   │   └── AppRole.cs
│   ├── MasterData/
│   ├── Inventory/
│   ├── Inbound/
│   ├── Outbound/
│   └── Internal/
├── Events/                # IDomainEvent, domain event classes
└── Interfaces/
    ├── IRepository.cs     # Generic: GetById, Add, Update, Delete
    ├── IUnitOfWork.cs
    └── Repositories/      # Specific repository interfaces
```

#### 1.4 Application Layer
```
WMS.Application/
├── Common/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   ├── UnitOfWorkBehavior.cs
│   │   └── ExceptionBehavior.cs
│   └── Interfaces/        # ICurrentUserService, IFileStorageService, IEmailService
├── DTOs/                  # Response DTOs per feature
└── Features/              # Feature folders (see per-phase breakdown)
```

#### 1.5 Infrastructure Layer
```
WMS.Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs            # IdentityDbContext<AppUser, AppRole, Guid>
│   │                              # OnModelCreating: rename Identity tables, all entity configs
│   ├── Configurations/            # IEntityTypeConfiguration<T> per entity
│   └── Repositories/              # Concrete repository implementations
├── Identity/
│   └── CurrentUserService.cs
├── Services/
│   └── FileStorageService.cs
└── DependencyInjection.cs
```

**`AppDbContext.cs` Identity rename (critical):**
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.Entity<AppUser>().ToTable("Users");
    builder.Entity<AppRole>().ToTable("Roles");
    builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
    builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
    builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
    builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
    builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
```

#### 1.6 Roundhouse Migration Project
```
WMS.Migration/
├── Program.cs             # RoundhouseRunner entrypoint
├── appsettings.json       # Connection string
├── Scripts/               # DDL: CREATE TABLE, ALTER, INDEX (run first)
│   ├── 0001_create_identity_tables.sql
│   ├── 0002_create_master_data_tables.sql
│   ├── 0003_create_inventory_tables.sql
│   ├── 0004_create_inbound_tables.sql
│   ├── 0005_create_outbound_tables.sql
│   └── 0006_create_internal_tables.sql
├── SeedData/              # Required reference data (run second)
│   ├── 0001_seed_roles.sql
│   ├── 0002_seed_admin_user.sql
│   ├── 0003_seed_units.sql
│   └── 0004_seed_app_settings.sql
└── SampleData/            # Demo/dev data (run third)
    ├── 0001_sample_warehouses.sql
    ├── 0002_sample_suppliers_customers.sql
    ├── 0003_sample_categories_products.sql
    └── 0004_sample_stock.sql
```

**Roundhouse runner configuration:**
```csharp
// WMS.Migration/Program.cs
var config = new RoundhouseConfiguration
{
    ConnectionString = connectionString,
    SqlFilesDirectory = ".",
    UpFolderName = "Scripts",
    // Run order: Scripts → SeedData → SampleData
    // Achieved via Roundhouse's folder ordering + naming convention
};
```

**`Program.cs` pattern with ordered folders:**
```csharp
using roundhouse;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.containers;

var connectionString = args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? throw new InvalidOperationException("No connection string provided.");

// Run Scripts (DDL)
await RunFolder(connectionString, "Scripts");
// Run SeedData
await RunFolder(connectionString, "SeedData");
// Run SampleData
await RunFolder(connectionString, "SampleData");

static async Task RunFolder(string connStr, string folder)
{
    Console.WriteLine($"[Migration] Running folder: {folder}");
    Migrate.The.Database(m => m
        .FromPath(folder)
        .WithConnectionString(connStr)
        .WithDatabaseType(DatabaseType.PostgreSQL)
    );
}
```

#### 1.7 Blazor Presentation Bootstrap
```
WMS.Presentation/
├── Program.cs             # AddRazorComponents, AddServerSideBlazor, Identity, MediatR, DI
├── appsettings.json
├── App.razor
├── Routes.razor
├── Layout/
│   ├── MainLayout.razor   # Sidebar + TopBar shell
│   ├── AuthLayout.razor   # Login/Register layout
│   ├── NavMenu.razor      # Role-aware navigation
│   └── TopBar.razor
├── Components/
│   └── Shared/            # Spinner, Alert, Pagination, Confirm dialog, Badge
├── Pages/
│   └── Auth/
│       ├── Login.razor
│       └── Logout.razor
└── wwwroot/
    ├── css/
    │   └── app.css
    └── uploads/           # Mounted named Docker volume
```

**`Program.cs` essentials:**
```csharp
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddIdentity<AppUser, AppRole>(options => {
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IIdGenerator, UuidV7Generator>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.AddForwardedHeaders(); // For reverse-proxy SSL termination

// Pipeline behaviors (order matters)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
```

#### 1.8 Docker Setup
```
docker-compose.yml
Dockerfile.app
Dockerfile.migration
.dockerignore
```

**`docker-compose.yml`:**
```yaml
version: "3.9"
services:
  db:
    image: postgres:16
    environment:
      POSTGRES_DB: wms_db
      POSTGRES_USER: wms_user
      POSTGRES_PASSWORD: wms_pass
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U wms_user"]
      interval: 5s
      retries: 10

  migrate:
    build:
      context: .
      dockerfile: Dockerfile.migration
    depends_on:
      db:
        condition: service_healthy
    environment:
      CONNECTION_STRING: "Host=db;Port=5432;Database=wms_db;Username=wms_user;Password=wms_pass"

  app:
    build:
      context: .
      dockerfile: Dockerfile.app
    ports:
      - "5000:8080"
    depends_on:
      migrate:
        condition: service_completed_successfully
    environment:
      ConnectionStrings__Default: "Host=db;Port=5432;Database=wms_db;Username=wms_user;Password=wms_pass"
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:8080
    volumes:
      - uploads:/app/wwwroot/uploads

volumes:
  pgdata:
  uploads:
```

**Note:** No `UseHttpsRedirection()` / `UseHsts()` in `Program.cs`. SSL is terminated at the reverse proxy (Nginx/Caddy).

#### 1.9 Phase 1 Deliverables Checklist
- [ ] Solution builds with no errors
- [ ] All 6 projects reference correctly (no circular deps)
- [ ] `docker-compose up` spins db → migrate → app in order
- [ ] Roundhouse runs Scripts → SeedData → SampleData successfully
- [ ] Identity tables renamed (no `AspNet` prefix in DB)
- [ ] Admin user seeded via SeedData SQL
- [ ] Login page renders and authenticates admin user
- [ ] Cookie auth works; unauthorized redirect to `/login`

---

### Phase 2 — Master Data Management
**Goal:** Full CRUD for all reference/lookup entities with role-based access.

#### 2.1 Features

**Warehouses & Zones & Locations**
```
Application/Features/Warehouses/
├── Commands/
│   ├── CreateWarehouse/ → CreateWarehouseCommand, Handler, Validator
│   ├── UpdateWarehouse/ → UpdateWarehouseCommand, Handler, Validator
│   └── ToggleWarehouseStatus/ → ToggleWarehouseStatusCommand, Handler
├── Queries/
│   ├── GetWarehouses/   → GetWarehousesQuery, Handler → PagedResult<WarehouseDto>
│   └── GetWarehouseById/ → GetWarehouseByIdQuery, Handler → WarehouseDetailDto
└── DTOs/
    ├── WarehouseDto.cs
    └── WarehouseDetailDto.cs
```

Same pattern repeated for: `Zones`, `Locations`, `Suppliers`, `Customers`, `Categories`, `Units`, `Products`.

#### 2.2 Pages
```
Pages/
├── Warehouses/
│   ├── WarehouseList.razor      # DataGrid, search, filter, pagination
│   └── WarehouseForm.razor      # Create/Edit modal or inline form
├── Zones/
├── Locations/
├── Suppliers/
├── Customers/
├── Categories/
│   └── CategoryTree.razor       # Tree view for hierarchy
├── Units/
└── Products/
    ├── ProductList.razor
    ├── ProductForm.razor        # Includes image upload
    └── ProductDetail.razor      # Stock summary per location
```

#### 2.3 Shared Components Built in This Phase
- `DataTable<TItem>` — sortable, paginated, searchable generic table component
- `ConfirmDialog` — reusable delete confirmation modal
- `StatusBadge` — Active/Inactive pill
- `ImageUpload` — single file upload with preview, saves to `wwwroot/uploads/products/`
- `TreeSelect` — for Category hierarchy selection

#### 2.4 Database Scripts Added (Scripts/)
```sql
-- 0002_create_master_data_tables.sql
CREATE TABLE "Warehouses" (
    "Id"        UUID PRIMARY KEY,
    "Code"      VARCHAR(50) NOT NULL UNIQUE,
    "Name"      VARCHAR(200) NOT NULL,
    "Address"   TEXT,
    "IsActive"  BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    "UpdatedAt" TIMESTAMPTZ,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE "Zones" (
    "Id"          UUID PRIMARY KEY,
    "WarehouseId" UUID NOT NULL REFERENCES "Warehouses"("Id"),
    "Code"        VARCHAR(50) NOT NULL,
    "Name"        VARCHAR(200) NOT NULL,
    "ZoneType"    VARCHAR(50) NOT NULL,
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"   TIMESTAMPTZ NOT NULL,
    "UpdatedAt"   TIMESTAMPTZ,
    "IsDeleted"   BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE "Locations" (
    "Id"          UUID PRIMARY KEY,
    "ZoneId"      UUID NOT NULL REFERENCES "Zones"("Id"),
    "Aisle"       VARCHAR(20),
    "Bay"         VARCHAR(20),
    "Level"       VARCHAR(20),
    "Position"    VARCHAR(20),
    "Barcode"     VARCHAR(100),
    "MaxCapacity" DECIMAL(18,4),
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"   TIMESTAMPTZ NOT NULL,
    "UpdatedAt"   TIMESTAMPTZ,
    "IsDeleted"   BOOLEAN NOT NULL DEFAULT FALSE
);
-- ... Suppliers, Customers, Categories, Units, Products tables
```

#### 2.5 Phase 2 Deliverables Checklist
- [ ] CRUD for all 9 master data entities
- [ ] Soft delete on all entities (IsDeleted flag, global query filter in EF)
- [ ] Audit trail on create/update (CreatedBy, UpdatedBy from `ICurrentUserService`)
- [ ] Product image upload working (relative path stored in DB)
- [ ] Category tree navigation renders correctly
- [ ] Data validation errors surface as inline field messages (FluentValidation → Blazor)
- [ ] Role guard: only Admin/WarehouseManager can create/edit/delete master data

---

### Phase 3 — Inventory & Stock Management
**Goal:** Real-time stock visibility per product per location; manual stock adjustment workflow.

#### 3.1 Features
```
Application/Features/Stock/
├── Queries/
│   ├── GetStockByProduct/        → ProductStockSummaryDto (qty, reserved, available)
│   ├── GetStockByLocation/       → LocationStockDto
│   ├── GetStockMovements/        → PagedResult<StockMovementDto> with filters
│   └── GetLowStockReport/        → Products below ReorderPoint
├── Commands/
│   └── (Stock mutations happen as side-effects of GR/GI/Transfer handlers)
```

```
Application/Features/StockAdjustment/
├── Commands/
│   ├── CreateStockAdjustment/    → Draft
│   ├── SubmitAdjustmentForApproval/
│   ├── ApproveStockAdjustment/   → Applies delta to Stock table + StockMovement row
│   └── RejectStockAdjustment/
├── Queries/
│   ├── GetAdjustments/           → PagedResult<AdjustmentDto>
│   └── GetAdjustmentById/        → AdjustmentDetailDto with items
└── DTOs/
```

#### 3.2 Pages
```
Pages/Inventory/
├── StockOverview.razor      # Filterable grid: Product | Location | OnHand | Reserved | Available
├── StockByLocation.razor    # Visual warehouse layout → zone → location drill-down
├── StockMovements.razor     # Audit trail of all movements with filters
├── LowStockReport.razor     # Products at/below reorder point, export CSV
└── StockAdjustment/
    ├── AdjustmentList.razor
    ├── AdjustmentForm.razor  # Add items, enter actual vs system qty
    └── AdjustmentApproval.razor  # Manager approval screen
```

#### 3.3 Database Scripts Added
```sql
-- 0003_create_inventory_tables.sql
CREATE TABLE "Stock" (
    "Id"               UUID PRIMARY KEY,
    "ProductId"        UUID NOT NULL REFERENCES "Products"("Id"),
    "LocationId"       UUID NOT NULL REFERENCES "Locations"("Id"),
    "Quantity"         DECIMAL(18,4) NOT NULL DEFAULT 0,
    "ReservedQuantity" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "LastUpdatedAt"    TIMESTAMPTZ NOT NULL,
    CONSTRAINT uq_stock_product_location UNIQUE ("ProductId","LocationId")
);

CREATE TABLE "StockMovements" (
    "Id"             UUID PRIMARY KEY,
    "ProductId"      UUID NOT NULL REFERENCES "Products"("Id"),
    "FromLocationId" UUID REFERENCES "Locations"("Id"),
    "ToLocationId"   UUID REFERENCES "Locations"("Id"),
    "Quantity"       DECIMAL(18,4) NOT NULL,
    "MovementType"   VARCHAR(50) NOT NULL,
    "ReferenceNo"    VARCHAR(100),
    "Note"           TEXT,
    "CreatedBy"      UUID NOT NULL,
    "CreatedAt"      TIMESTAMPTZ NOT NULL
);

CREATE TABLE "StockAdjustments" ( ... );
CREATE TABLE "StockAdjustmentItems" ( ... );
```

#### 3.4 Phase 3 Deliverables Checklist
- [ ] Stock table updated atomically on every movement (optimistic concurrency via `xmin` or `RowVersion`)
- [ ] Available qty = `Quantity - ReservedQuantity` computed property shown everywhere
- [ ] Low stock alert badge in nav when any product below reorder point
- [ ] Adjustment approval flow enforces role: only WarehouseManager/Admin can approve
- [ ] All stock mutations create a `StockMovement` row (immutable audit trail)
- [ ] Export to CSV on LowStockReport

---

### Phase 4 — Inbound (Purchase Orders & Goods Receipt)
**Goal:** Full PO lifecycle from draft to fully received, with auto-stock update.

#### 4.1 Features
```
Application/Features/PurchaseOrders/
├── Commands/
│   ├── CreatePurchaseOrder/      # Draft
│   ├── UpdatePurchaseOrder/      # Editable in Draft only
│   ├── ConfirmPurchaseOrder/     # → Confirmed status
│   └── CancelPurchaseOrder/
├── Queries/
│   ├── GetPurchaseOrders/        # Paged, filterable by status/supplier/date
│   └── GetPurchaseOrderById/     # Full detail with items

Application/Features/GoodsReceipt/
├── Commands/
│   ├── CreateGoodsReceipt/       # From confirmed PO, auto-populates items
│   ├── UpdateGoodsReceiptItem/   # Set received qty, location, batch, expiry
│   └── CompleteGoodsReceipt/     # Validates all items, increments Stock, creates StockMovements
├── Queries/
│   ├── GetGoodsReceipts/
│   └── GetGoodsReceiptById/
```

#### 4.2 State Machine — PurchaseOrder
```
Draft → Confirmed → PartialReceived → FullyReceived
      ↘ Cancelled (from Draft or Confirmed only)
```

#### 4.3 CompleteGoodsReceipt Handler Logic
```csharp
// 1. Validate all items have ReceivedQty > 0
// 2. For each item:
//    a. Upsert Stock (ProductId + LocationId)
//    b. Insert StockMovement (MovementType = Receipt)
// 3. Update PurchaseOrderItem.ReceivedQty
// 4. Recalculate PO status: PartialReceived or FullyReceived
// 5. Set GoodsReceipt.Status = Completed
// All inside UnitOfWork → single transaction
```

#### 4.4 Pages
```
Pages/Inbound/
├── PurchaseOrders/
│   ├── POList.razor          # Status-filtered tabs (All | Draft | Confirmed | Received)
│   ├── POForm.razor          # Create/edit PO with line items
│   └── PODetail.razor        # Timeline, items, linked GRs
└── GoodsReceipt/
    ├── GRList.razor
    ├── GRForm.razor          # Select location per item, scan barcode, enter batch/expiry
    └── GRDetail.razor
```

#### 4.5 Phase 4 Deliverables Checklist
- [ ] PO number auto-generated: `PO-YYYYMM-NNNNN`
- [ ] GR number auto-generated: `GR-YYYYMM-NNNNN`
- [ ] Completing a GR correctly increments stock and creates movement rows
- [ ] PO status auto-transitions based on cumulative received quantities
- [ ] Receiver role can create/complete GRs but not confirm/cancel POs
- [ ] Barcode scan input on GR form (keyboard wedge compatible via `@onkeydown`)

---

### Phase 5 — Outbound (Sales Orders, Pick Lists & Goods Issue)
**Goal:** SO lifecycle, pick task assignment, and goods issue with stock reservation.

#### 5.1 Features
```
Application/Features/SalesOrders/
├── Commands/
│   ├── CreateSalesOrder/
│   ├── UpdateSalesOrder/
│   ├── ConfirmSalesOrder/        # Checks available stock; reserves quantity
│   └── CancelSalesOrder/         # Releases reservations

Application/Features/PickList/
├── Commands/
│   ├── GeneratePickList/         # From confirmed SO; suggests optimal locations (FIFO)
│   ├── AssignPickList/           # Assign to Picker user
│   ├── UpdatePickedQty/          # Picker updates item by item
│   └── CompletePickList/         # All items picked → trigger GoodsIssue creation

Application/Features/GoodsIssue/
├── Commands/
│   ├── CompleteGoodsIssue/       # Decrements Stock, releases reservation, creates movements
│   └── CancelGoodsIssue/
├── Queries/
│   └── GetGoodsIssueById/
```

#### 5.2 State Machine — SalesOrder
```
Draft → Confirmed → PartialPicked → FullyPicked → Shipped
      ↘ Cancelled (Draft or Confirmed)
```

#### 5.3 Stock Reservation Logic
```
OnConfirmSalesOrder:
  foreach item:
    if Stock.AvailableQty >= item.OrderedQty → Stock.ReservedQuantity += OrderedQty
    else → throw InsufficientStockException (surfaces as validation failure)

OnCancelSalesOrder:
  foreach item: Stock.ReservedQuantity -= OrderedQty (clamped to 0)
```

#### 5.4 FIFO Location Suggestion (GeneratePickList)
```csharp
// For each SO item, find locations with stock ordered by:
// 1. StockMovements.CreatedAt ASC (oldest receipt first)
// 2. Fill from one location; spill to next if insufficient
// → Creates PickListItems with suggested LocationId + qty split
```

#### 5.5 Pages
```
Pages/Outbound/
├── SalesOrders/
│   ├── SOList.razor
│   ├── SOForm.razor
│   └── SODetail.razor
├── PickLists/
│   ├── PickListDashboard.razor   # Kanban: Pending | InProgress | Completed
│   ├── PickListDetail.razor      # Picker's work screen; check off items, confirm qty
│   └── AssignPicker.razor
└── GoodsIssue/
    ├── GIList.razor
    └── GIDetail.razor
```

#### 5.6 Phase 5 Deliverables Checklist
- [ ] SO number auto-generated: `SO-YYYYMM-NNNNN`
- [ ] Stock reservation prevents double-selling
- [ ] FIFO pick suggestion implemented and working
- [ ] Picker dashboard shows only their assigned pick lists
- [ ] Completing GI decrements stock and releases reservation atomically
- [ ] SO status auto-transitions (PartialPicked / FullyPicked / Shipped)

---

### Phase 6 — Transfer Orders & Returns
**Goal:** Inter-warehouse transfers; supplier returns and customer returns.

#### 6.1 Transfer Orders
```
Application/Features/TransferOrders/
├── Commands/
│   ├── CreateTransferOrder/
│   ├── ApproveTransferOrder/
│   ├── StartTransfer/           # Status → InTransit; reserves source stock
│   └── CompleteTransfer/        # Moves stock: source location ↓, target location ↑
└── Queries/ ...
```

**State Machine:**
```
Draft → Approved → InTransit → Completed
      ↘ Cancelled
```

#### 6.2 Returns
```
Application/Features/Returns/
├── Commands/
│   ├── CreateSupplierReturn/    # Decrements stock; references original GR
│   ├── CreateCustomerReturn/    # Increments stock; references original GI; inspection note
│   └── ProcessReturn/           # Final stock update + movements
└── Queries/ ...
```

#### 6.3 Pages
```
Pages/Transfers/
├── TransferList.razor
└── TransferForm.razor

Pages/Returns/
├── SupplierReturns.razor
└── CustomerReturns.razor
```

#### 6.4 Phase 6 Deliverables Checklist
- [ ] Transfer between different warehouses creates correct movement records for both source and destination
- [ ] Approval required for transfers (WarehouseManager+)
- [ ] Customer returns increment stock only after inspection status is set to Accepted
- [ ] All returns linked to original reference documents

---

### Phase 7 — Dashboard, Reports & User Management
**Goal:** Executive dashboard with KPIs, filterable reports, user/role management.

#### 7.1 Dashboard KPIs (ApexCharts)
```
Components/Dashboard/
├── KpiCard.razor              # Total Products, Low Stock Count, Pending POs, Open SOs
├── StockValueChart.razor      # Bar chart: stock value by category (ApexCharts)
├── InboundOutboundChart.razor # Line chart: GR vs GI volume last 30 days
├── TopProductsChart.razor     # Donut: top 10 products by movement volume
└── RecentMovements.razor      # Live-updating table (SignalR / periodic refresh)
```

#### 7.2 Reports
```
Pages/Reports/
├── StockSnapshot.razor         # As-of-date stock levels; export Excel/CSV
├── MovementHistory.razor       # Filter by product/location/type/date; export
├── InboundReport.razor         # GR summary by supplier/period
├── OutboundReport.razor        # GI summary by customer/period
├── AdjustmentReport.razor      # All adjustments with approver
└── AuditLog.razor              # Full audit trail (entity, user, old/new JSON diff)
```

#### 7.3 User Management
```
Pages/UserManagement/
├── UserList.razor              # All users, status, role badges
├── UserForm.razor              # Create/edit; assign roles; reset password
└── RoleList.razor              # View roles (roles are static/seeded; no UI CRUD)
```

#### 7.4 Settings
```
Pages/Settings/
├── AppSettings.razor           # Key-value config table (editable by Admin)
└── ChangePassword.razor        # Self-service password change
```

#### 7.5 Phase 7 Deliverables Checklist
- [ ] Dashboard loads within 2s (queries optimized with indexes)
- [ ] ApexCharts renders without flicker on Blazor Server re-render (JS interop guard)
- [ ] All reports support date range filter
- [ ] Export to CSV functional on all report pages
- [ ] Admin can create users, assign roles, deactivate accounts
- [ ] Audit log captures all create/update/delete with before/after JSON (via EF SaveChanges interceptor or MediatR behavior)

---

### Phase 8 — Hardening, Production Readiness & Deployment
**Goal:** Security, performance, observability, and VPS deployment.

#### 8.1 Security Hardening
- `[Authorize(Roles = "...")]` on all pages and `AuthorizeRouteView` in router
- Anti-forgery protection (built-in Blazor Server)
- Password policy: min 8 chars, require digit + special
- Account lockout after 5 failed attempts (`options.Lockout`)
- `HttpOnly` + `Secure` + `SameSite=Strict` cookie flags
- Input sanitization: all text inputs pass through HtmlEncoder before display

#### 8.2 Performance
- Global query filters for soft delete on all entities
- PostgreSQL indexes on FK columns, status columns, and date columns
- `IQueryable` projection to DTOs (no loading full entities for list views)
- `AsNoTracking()` on all Query handlers
- Blazor component `ShouldRender()` override on heavy dashboard components

#### 8.3 Observability
- Structured logging via `Microsoft.Extensions.Logging` → stdout (Docker captures)
- `LoggingBehavior` logs every Command/Query with duration
- Health check endpoint: `app.MapHealthChecks("/health")`
- `ExceptionBehavior` in pipeline: catches unhandled exceptions, logs, returns `Result.Fail`

#### 8.4 Docker Production Build
```dockerfile
# Dockerfile.app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish WMS.Presentation/WMS.Presentation.csproj -c Release -o /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "WMS.Presentation.dll"]
```

```dockerfile
# Dockerfile.migration
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /migration
COPY WMS.Migration/bin/Release/net10.0/publish .
ENTRYPOINT ["dotnet", "WMS.Migration.dll"]
```

#### 8.5 Reverse Proxy (Nginx example)
```nginx
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl;
    server_name yourdomain.com;
    ssl_certificate     /etc/ssl/certs/yourdomain.crt;
    ssl_certificate_key /etc/ssl/private/yourdomain.key;

    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection "upgrade";   # Required for Blazor SignalR
        proxy_set_header   Host $host;
        proxy_set_header   X-Real-IP $remote_addr;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

**Note:** `proxy_http_version 1.1` + `Upgrade`/`Connection` headers are **required** for Blazor Server's SignalR WebSocket connection.

#### 8.6 `Program.cs` — Production-safe headers
```csharp
// No UseHttpsRedirection / UseHsts — SSL is at reverse proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
```

#### 8.7 Phase 8 Deliverables Checklist
- [ ] All pages protected by role-based authorization; unauthorized access returns 403
- [ ] Docker production build passes with no warnings
- [ ] `docker-compose up` on VPS deploys successfully end-to-end
- [ ] Blazor SignalR connection stable through Nginx proxy (WebSocket upgrade works)
- [ ] SSL via hosting-level reverse proxy; no cert inside app container
- [ ] Health check endpoint accessible and returns 200
- [ ] Roundhouse migration idempotent (safe to re-run on restart)
- [ ] Log output captured by Docker (`docker logs wms_app`)

---

## SQL Reference — All Migration Scripts Listing

### Scripts/ (DDL — run first)
| File | Content |
|---|---|
| `0001_create_identity_tables.sql` | Users, Roles, UserRoles, UserClaims, RoleClaims, UserLogins, UserTokens |
| `0002_create_master_data_tables.sql` | Warehouses, Zones, Locations, Suppliers, Customers, Categories, Units, Products |
| `0003_create_inventory_tables.sql` | Stock, StockMovements, StockAdjustments, StockAdjustmentItems |
| `0004_create_inbound_tables.sql` | PurchaseOrders, PurchaseOrderItems, GoodsReceipts, GoodsReceiptItems |
| `0005_create_outbound_tables.sql` | SalesOrders, SalesOrderItems, PickLists, PickListItems, GoodsIssues, GoodsIssueItems |
| `0006_create_internal_tables.sql` | TransferOrders, TransferOrderItems, Returns, AuditLog, AppSettings |

### SeedData/ (reference data — run second)
| File | Content |
|---|---|
| `0001_seed_roles.sql` | Insert Admin, WarehouseManager, Receiver, Picker, StockController, Viewer |
| `0002_seed_admin_user.sql` | Insert default admin account (password hashed) |
| `0003_seed_units.sql` | pcs, box, carton, pallet, kg, g, l, ml |
| `0004_seed_app_settings.sql` | Company name, address, default currency, low-stock threshold |

### SampleData/ (dev/demo data — run third)
| File | Content |
|---|---|
| `0001_sample_warehouses.sql` | 2 warehouses with zones and locations |
| `0002_sample_suppliers_customers.sql` | 5 suppliers, 5 customers |
| `0003_sample_categories_products.sql` | 3 categories, 20 products |
| `0004_sample_stock.sql` | Initial stock for sample products |

---

## NuGet Package Reference

| Package | Project | Purpose |
|---|---|---|
| `MediatR` | Application | CQRS mediator |
| `FluentValidation` | Application | Command/Query validation |
| `FluentValidation.DependencyInjectionExtensions` | Application | DI registration |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Infrastructure | PostgreSQL provider |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Infrastructure | Identity + EF |
| `Microsoft.EntityFrameworkCore` | Infrastructure | ORM (mapping/query only) |
| `Serilog.AspNetCore` | Presentation | Structured logging (optional) |
| `ApexCharts` (Blazor) | Presentation | Dashboard charts |
| `roundhouse` | Migration | SQL migration runner |

---

## Folder Structure — Final

```
WMS/
├── WMS.sln
├── docker-compose.yml
├── Dockerfile.app
├── Dockerfile.migration
├── .dockerignore
├── PLAN.md                                    ← this file
│
├── WMS.SharedKernel/
│   ├── BaseEntity.cs
│   ├── AuditableEntity.cs
│   ├── Result.cs
│   ├── IIdGenerator.cs
│   ├── UuidV7Generator.cs
│   ├── PagedResult.cs
│   └── Enums/
│
├── WMS.Domain/
│   ├── Entities/
│   │   ├── Identity/
│   │   ├── MasterData/
│   │   ├── Inventory/
│   │   ├── Inbound/
│   │   ├── Outbound/
│   │   └── Internal/
│   ├── Events/
│   └── Interfaces/
│
├── WMS.Application/
│   ├── Common/
│   │   ├── Behaviors/
│   │   └── Interfaces/
│   ├── DTOs/
│   └── Features/
│       ├── Auth/
│       ├── Warehouses/
│       ├── Zones/
│       ├── Locations/
│       ├── Suppliers/
│       ├── Customers/
│       ├── Categories/
│       ├── Units/
│       ├── Products/
│       ├── Stock/
│       ├── StockAdjustment/
│       ├── PurchaseOrders/
│       ├── GoodsReceipt/
│       ├── SalesOrders/
│       ├── PickList/
│       ├── GoodsIssue/
│       ├── TransferOrders/
│       ├── Returns/
│       ├── Reports/
│       └── UserManagement/
│
├── WMS.Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/
│   │   └── Repositories/
│   ├── Identity/
│   ├── Services/
│   └── DependencyInjection.cs
│
├── WMS.Presentation/
│   ├── Program.cs
│   ├── App.razor
│   ├── Routes.razor
│   ├── Layout/
│   ├── Components/
│   │   ├── Dashboard/
│   │   └── Shared/
│   ├── Pages/
│   │   ├── Auth/
│   │   ├── Warehouses/
│   │   ├── Zones/
│   │   ├── Locations/
│   │   ├── Suppliers/
│   │   ├── Customers/
│   │   ├── Categories/
│   │   ├── Units/
│   │   ├── Products/
│   │   ├── Inventory/
│   │   ├── Inbound/
│   │   ├── Outbound/
│   │   ├── Transfers/
│   │   ├── Returns/
│   │   ├── Reports/
│   │   ├── UserManagement/
│   │   └── Settings/
│   └── wwwroot/
│
└── WMS.Migration/
    ├── Program.cs
    ├── appsettings.json
    ├── Scripts/
    ├── SeedData/
    └── SampleData/
```

---

*Last updated: 2026-06-26 | Stack: .NET 10 · Blazor Server · PostgreSQL · Roundhouse · Clean Architecture*
