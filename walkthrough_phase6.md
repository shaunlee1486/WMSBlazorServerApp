# Walkthrough — Phase 6: Transfer Orders & Returns

We have completed the implementation of **Phase 6 — Transfer Orders & Returns**! The solution builds successfully with zero errors.

Here is a summary of what has been implemented and how to verify.

---

## 1. Domain Entities & Database Mapping (`WMS.Domain` & `WMS.Infrastructure`)

- **Domain Enums**: Added `ReturnType`, `ReturnStatus`, `InspectionStatus`, and `TransferOrderItemStatus` to [Enums.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.SharedKernel/Enums.cs).
- **Domain Entities**: Created in `WMS.Domain/Entities/Internal/`:
  - [TransferOrder.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Internal/TransferOrder.cs): Represents transfer requests between warehouses with state transitions (`Draft` -> `Approved` -> `InTransit` -> `Completed` / `Cancelled`).
  - [TransferOrderItem.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Internal/TransferOrderItem.cs): Lines containing product, source bin location, destination bin location, quantity, and status.
  - [Return.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Internal/Return.cs): Represents returns, distinguishing between Customer and Supplier types.
  - [ReturnItem.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Domain/Entities/Internal/ReturnItem.cs): Contains product, target location, returned quantity, and inspection outcomes.
- **Repositories**: Registered interfaces in `WMS.Domain/Interfaces/Repositories/`:
  - `ITransferOrderRepository` and `IReturnRepository`.
- **Infrastructure Configurations**:
  - [InternalConfigurations.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Infrastructure/Persistence/Configurations/InternalConfigurations.cs): Mapped database entities, including enum to string conversions, decimal scale setups, indexes, and cascades.
  - [AppDbContext.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Infrastructure/Persistence/AppDbContext.cs): Registered new sets and soft-delete query filters on Transfer Orders and Returns.
  - [EFTransferOrderRepository.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Infrastructure/Persistence/Repositories/EFTransferOrderRepository.cs) & [EFReturnRepository.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Infrastructure/Persistence/Repositories/EFReturnRepository.cs): Implemented the repository interfaces, including database sequence generator methods matching order patterns (`TO-yyyyMM-XXXXX` and `RET-yyyyMM-XXXXX`).
  - Registered concrete repositories in [DependencyInjection.cs](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Infrastructure/DependencyInjection.cs).

---

## 2. CQRS Commands & Queries (`WMS.Application`)

- **Transfer Order Features** (`WMS.Application/Features/TransferOrders/`):
  - `CreateTransferOrderCommand` & `UpdateTransferOrderCommand`: Creates or updates transfer orders in `Draft` state.
  - `ApproveTransferOrderCommand`: Progresses state to `Approved`.
  - `StartTransferCommand`: Transition to `InTransit`. Checks inventory and reserves stock at the source location.
  - `CompleteTransferCommand`: Deducts source stock, increments target stock, registers a single `Transfer` `StockMovement` entry, and sets state to `Completed`.
  - `CancelTransferCommand`: Cancels active transfers and releases stock reservations.
  - `GetTransferOrdersQuery` & `GetTransferOrderByIdQuery`
- **Return Features** (`WMS.Application/Features/Returns/`):
  - `CreateSupplierReturnCommand`: Validates stock availability and saves a supplier return request referencing a Goods Receipt.
  - `CreateCustomerReturnCommand`: Saves customer return referencing a Goods Issue with default `Pending` inspection status.
  - `UpdateReturnInspectionStatusCommand`: Records item-by-item inspection status (`Accepted` / `Rejected`).
  - `ProcessReturnCommand`:
    - Customer returns: Increments target stock *only* for items marked as `Accepted`.
    - Supplier returns: Decrements source stock.
    - Transitions return state to `Completed` and logs `Return` type `StockMovement` records.
  - `CancelReturnCommand`: Transitions state to `Cancelled`.
  - `GetReturnsQuery` & `GetReturnByIdQuery`

---

## 3. UI Presentation Layer (`WMS.Presentation`)

- **Routing & Navigation**:
  - Registered using statements in [_Imports.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/_Imports.razor).
  - Configured sidebar navigation in [NavMenu.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Layout/NavMenu.razor) with a new "Internal Operations" group containing "Transfer Orders" and "Returns Ledger".
- **Transfer Pages**:
  - [TransferList.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Transfers/TransferList.razor): Tabular status overview of transfers.
  - [TransferForm.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Transfers/TransferForm.razor): Routing setup form including source/destination warehouses, dynamic source/destination location filtering based on selected warehouses.
  - [TransferDetail.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Transfers/TransferDetail.razor): Overview panel displaying details and workflow controls (Approve -> Ship -> Receive).
- **Return Pages**:
  - [ReturnList.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Returns/ReturnList.razor): Returns grid.
  - [ReturnForm.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Returns/ReturnForm.razor): Form to input return type, original document reference numbers, products, locations, and return quantities.
  - [ReturnDetail.razor](file:///d:/DotNet/Blazor/Blazor%20Server/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Returns/ReturnDetail.razor): Return lifecycle overview page. For Customer returns, includes action buttons to dynamically inspect and `Accept` or `Reject` items line-by-line before finalizing.

---

## Verification Plan

### 1. Build Verification
Confirm clean compilation of the solution:
```powershell
dotnet build WMS.sln
```

### 2. Manual Verification Scenarios
1. Start services:
   ```powershell
   docker-compose up --build
   ```
2. Navigate to `http://localhost:5000/` and sign in.
3. **Inter-warehouse Transfer test**:
   - Go to **Transfer Orders**, click **New Transfer Order**.
   - Choose different source and destination warehouses, add an item (select a product and source location, destination location, and quantity). Click **Save Draft**.
   - In details, click **Approve**, then click **Ship** (verify stock reserved count increases in source location).
   - Click **Receive** (verify stock decrements at source, increments at destination, and a `Transfer` movement is logged in **Stock Movements** ledger).
4. **Customer Return test**:
   - Go to **Returns Ledger**, click **Customer Return**.
   - Input GI reference, add items, and click **Save Return Order**.
   - In details, inspect items: mark one as `Accepted` and another as `Rejected`.
   - Click **Process & Finalize** (verify only the `Accepted` item increments target location stock).
5. **Supplier Return test**:
   - Click **Supplier Return**, enter reference, select product/source bin, and click **Save**.
   - Click **Process & Finalize** (verify stock decrements and a return movement is registered).
