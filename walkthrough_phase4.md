# Walkthrough - WMS Phase 4 Inbound (Purchase Orders & Goods Receipt)

We have successfully completed the implementation of **Phase 4 — Inbound (Purchase Orders & Goods Receipt)**! The entire solution compiles successfully with zero warnings and zero errors.

Here is a summary of what has been implemented and how to verify.

---

## 1. Domain Entities & Database Configurations (WMS.Domain & WMS.Infrastructure)
- **Domain Entities**: Created in `WMS.Domain/Entities/Inbound/`:
  - [PurchaseOrder.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Domain/Entities/Inbound/PurchaseOrder.cs): Repesents purchase orders, linking to suppliers with status transitions (`Draft` -> `Confirmed` -> `PartialReceived`/`FullyReceived` -> `Cancelled`).
  - [PurchaseOrderItem.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Domain/Entities/Inbound/PurchaseOrderItem.cs): Contains ordered and cumulative received quantity details.
  - [GoodsReceipt.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Domain/Entities/Inbound/GoodsReceipt.cs): Represents a specific inventory receiving instance.
  - [GoodsReceiptItem.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Domain/Entities/Inbound/GoodsReceiptItem.cs): Records received count, allocated location, batch number, and expiry date.
- **Repository Contracts**: Registered interfaces in `WMS.Domain/Interfaces/Repositories/`:
  - `IPurchaseOrderRepository` & `IGoodsReceiptRepository`.
- **Infrastructure Mappings**:
  - [InboundConfigurations.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Infrastructure/Persistence/Configurations/InboundConfigurations.cs): Mapped tables with `(18, 4)` decimal precision, string enum conversions, and cascades.
  - [AppDbContext.cs](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Infrastructure/Persistence/AppDbContext.cs): Exposed DbSets and registered soft-delete query filters on PO and GR tables.
  - Registered concrete repository implementations (`EFPurchaseOrderRepository`, `EFGoodsReceiptRepository`) in `DependencyInjection.cs`.

---

## 2. Infrastructure & Application CQRS (WMS.Infrastructure & WMS.Application)
- **Purchase Order Features** (`WMS.Application/Features/PurchaseOrders/`):
  - `CreatePurchaseOrderCommand`: Safely drafts new purchase orders with monthly-sequential number formats (`PO-yyyyMM-XXXXX`).
  - `UpdatePurchaseOrderCommand`: Allows editing PO details in `Draft` state.
  - `ConfirmPurchaseOrderCommand`: Locks in drafts for receiving (`Confirmed`).
  - `CancelPurchaseOrderCommand`: Cancels confirmed POs.
  - `GetPurchaseOrdersQuery` & `GetPurchaseOrderByIdQuery`: Handles paginated list searches and details.
- **Goods Receipt Features** (`WMS.Application/Features/GoodsReceipts/`):
  - `CreateGoodsReceiptCommand`: Drafts new receipts linked to active POs.
  - `CompleteGoodsReceiptCommand`: Atomic database transaction handler that:
    - Increments stock levels in `Stock` table.
    - Records immutable audit history rows in `StockMovement` as `Receipt`.
    - Increments `ReceivedQty` on linked PO items.
    - Recalculates PO status (`PartialReceived` or `FullyReceived`).
    - Marks Goods Receipt as `Completed`.
  - `GetGoodsReceiptsQuery` & `GetGoodsReceiptByIdQuery`: Handles paging and details.

---

## 3. UI Presentation Layer (WMS.Presentation)
- **Purchase Order Pages**:
  - [POList.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Inbound/PurchaseOrders/POList.razor): Filters orders via tabs (All, Draft, Confirmed, Partial, Fully Received, Cancelled).
  - [POForm.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Inbound/PurchaseOrders/POForm.razor): Form to manage suppliers, products, ordered amounts, and prices.
  - [PODetail.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Inbound/PurchaseOrders/PODetail.razor): Summarizes lines, totals, and lifecycle trigger actions.
- **Goods Receipt Pages**:
  - [GRList.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Inbound/GoodsReceipts/GRList.razor): Lists active receipts and statuses.
  - [GRForm.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Inbound/GoodsReceipts/GRForm.razor): Panel enabling users to check off incoming shipments, select location bins, and enter batch/expiry dates.
  - [GRDetail.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Pages/Inbound/GoodsReceipts/GRDetail.razor): Audit summary for completed receipts.
- **Layout Shell**:
  - [NavMenu.razor](file:///d:/Shaun/BlazorApp/WMSBlazorServerApp/WMS.Presentation/Components/Layout/NavMenu.razor): Registered "Inbound" menu group highlighting links to Purchase Orders and Goods Receipts.

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
2. Navigate to `http://localhost:5000/inbound/purchase-orders` and sign in.
3. Click **New Purchase Order**.
4. Select a Supplier, add a product, input Ordered Qty and Unit Price, and click **Save Draft**.
5. Locate the draft in the PO list, click **Confirm** (status updates to `Confirmed`).
6. Click **Receive Goods** (either in the PO List actions or PODetail view).
7. In the Goods Receipt form:
   - Select an allocation Location for the product line.
   - Set a Receipt Qty (defaults to PO remaining amount).
   - Enter a Batch number and an Expiry date.
   - Click **Complete Receipt**.
8. Verify that:
   - The Goods Receipt is marked as `Completed`.
   - The Purchase Order status transitions to `FullyReceived` (or `PartialReceived` if you received less than ordered).
   - Navigate to **Stock Overview** (`/inventory`) and check that the stock level for the product has incremented.
   - Navigate to **Stock Movements** (`/inventory/movements`) and confirm a `Receipt` audit record exists referencing the GR serial number.
