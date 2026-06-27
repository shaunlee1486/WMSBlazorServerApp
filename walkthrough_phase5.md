# Walkthrough — Phase 5: Outbound (Sales Orders, Pick Lists & Goods Issue)

We have successfully completed the implementation of **Phase 5 — Outbound (Sales Orders, Pick Lists & Goods Issue)**! The entire solution compiles successfully with zero warnings and zero errors.

Here is a summary of what has been implemented and how to verify.

---

## 1. Domain Entities & Database Configurations (`WMS.Domain` & `WMS.Infrastructure`)

- **Domain Entities**: Created in `WMS.Domain/Entities/Outbound/`:
  - [SalesOrder.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Outbound/SalesOrder.cs): Represents sales orders, linking to customers with status transitions (`Draft` -> `Confirmed` -> `PartialPicked` / `FullyPicked` -> `Shipped` -> `Cancelled`).
  - [SalesOrderItem.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Outbound/SalesOrderItem.cs): Contains ordered and picking progress quantity details.
  - [PickList.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Outbound/PickList.cs): Represents picking requests assigned to warehouse operators.
  - [PickListItem.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Outbound/PickListItem.cs): Tracks picking progress line-by-line per location.
  - [GoodsIssue.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Outbound/GoodsIssue.cs): Represents an outbound stock dispatch.
  - [GoodsIssueItem.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Outbound/GoodsIssueItem.cs): Records outbound dispatch items, batch numbers, and stock sources.
- **Repository Contracts**: Registered interfaces in `WMS.Domain/Interfaces/Repositories/`:
  - `ISalesOrderRepository`, `IPickListRepository`, and `IGoodsIssueRepository`.
- **Infrastructure Mappings**:
  - [OutboundConfigurations.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Infrastructure/Persistence/Configurations/OutboundConfigurations.cs): Mapped tables with `(18, 4)` decimal precision, string enum conversions, indexes, and cascading deletes.
  - [AppDbContext.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Infrastructure/Persistence/AppDbContext.cs): Exposed DbSets and registered soft-delete query filters on Sales Orders, Pick Lists, and Goods Issues.
  - Registered concrete repository implementations (`EFSalesOrderRepository`, `EFPickListRepository`, `EFGoodsIssueRepository`) in [DependencyInjection.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Infrastructure/DependencyInjection.cs).

---

## 2. CQRS Commands & Queries (`WMS.Application`)

- **Sales Order Features** (`WMS.Application/Features/SalesOrders/`):
  - `CreateSalesOrderCommand`: Drafts new sales orders with monthly-sequential number formats (`SO-yyyyMM-XXXXX`).
  - `UpdateSalesOrderCommand`: Allows editing SO details in `Draft` state.
  - `ConfirmSalesOrderCommand`: Checks available stock globally, and allocates reservations to specific location bins using FIFO (oldest receipt first) by incrementing `Stock.ReservedQuantity`.
  - `CancelSalesOrderCommand`: Releases reservations by decrementing `Stock.ReservedQuantity` and cancels active pick lists.
  - `GetSalesOrdersQuery` & `GetSalesOrderByIdQuery`: Handles paginated list searches and details.
- **Pick List Features** (`WMS.Application/Features/PickList/`):
  - `GeneratePickListCommand`: Builds pick list checklist using FIFO suggestions.
  - `AssignPickListCommand`: Assigns task to operators.
  - `UpdatePickedQtyCommand`: Increments picked quantity line-by-line.
  - `CompletePickListCommand`: Updates sales order picked counts and spawns a draft Goods Issue.
- **Goods Issue Features** (`WMS.Application/Features/GoodsIssue/`):
  - `CompleteGoodsIssueCommand`: Decouples stock, atomically decrements physical stock and releases reservation, inserts `StockMovement` records as `Issue`, and sets SO status to `Shipped`.
  - `CancelGoodsIssueCommand`: Reverts Goods Issue.
  - `GetGoodsIssuesQuery` & `GetGoodsIssueByIdQuery`: Handles list searches and details.

---

## 3. UI Presentation Layer (`WMS.Presentation`)

- **Sales Order Pages**:
  - [SOList.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Outbound/SalesOrders/SOList.razor): Filters orders via status tabs.
  - [SOForm.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Outbound/SalesOrders/SOForm.razor): Form to manage customer selection, products, ordered amounts, and prices.
  - [SODetail.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Outbound/SalesOrders/SODetail.razor): Details card featuring lifecycle trigger actions.
- **Pick List Pages**:
  - [PickListDashboard.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Outbound/PickLists/PickListDashboard.razor): Kanban-styled tabular lists.
  - [PickListDetail.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Outbound/PickLists/PickListDetail.razor): Picker checklist panel to confirm quantities.
  - [AssignPicker.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Outbound/PickLists/AssignPicker.razor): Modal popup to allocate task.
- **Goods Issue Pages**:
  - [GIList.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Outbound/GoodsIssue/GIList.razor): Lists active dispatches.
  - [GIDetail.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Outbound/GoodsIssue/GIDetail.razor): Form to input batch numbers and complete dispatches.
- **Layout Shell**:
  - [NavMenu.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Layout/NavMenu.razor): Added "Outbound" menu group highlighting links to Sales Orders, Pick Lists, and Goods Issues.
- **Imports**:
  - [_Imports.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/_Imports.razor): Declared global namespaces for clean page compile resolution.

---

## Verification Plan

### 1. Build Verification
Confirm clean compilation of the solution:
```powershell
dotnet build WMS.sln
```

### 2. Manual Verification Scenario
1. Start the Docker services:
   ```powershell
   docker-compose up --build
   ```
2. Navigate to `http://localhost:5000/outbound/sales-orders` and sign in.
3. Click **New Sales Order**.
4. Select a Customer, add a product, input Ordered Qty and Unit Price, and click **Save Draft**.
5. Locate the draft in the SO list, click **Confirm** (status updates to `Confirmed`).
6. Click **Generate Pick** to create the picking request (status updates to `FullyPicked` once picking is complete).
7. Go to **Pick Lists** dashboard, assign picker, start picking, enter picked quantity, and complete picking.
8. Go to **Goods Issues**, input batch numbers, and click **Complete Dispatch Issue**.
9. Verify stock decrement in **Stock Overview** and audit movement creation in **Stock Movements**.
