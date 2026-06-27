# Walkthrough — Phase 7: Dashboard, Reports & User Management

We have successfully implemented and verified **Phase 7 — Dashboard, Reports & User Management**! The entire solution compiles cleanly with zero errors and zero warnings.

Here is a summary of what has been implemented and how to verify.

---

## 1. Core Analytics Dashboard (`Home.razor` & `WMS.Presentation/Components/Dashboard/`)

- **Dynamic KPI cards**: Fetches total active products, low stock alerts, pending POs, and open SOs.
- **Stock Value by Category (`StockValueChart.razor`)**: Apex bar chart displaying total stock value grouped by product category, calculating estimates dynamically using purchase order item cost history (falling back to a $15 base cost).
- **Fulfillment Timeline (`InboundOutboundChart.razor`)**: Area line chart detailing daily Goods Receipt (Inbound) and Goods Issue (Outbound) volume totals for the last 30 days.
- **Top Products (`TopProductsChart.razor`)**: Donut chart highlighting the top 10 products with the highest movement transaction volumes.
- **Activity Feed (`RecentMovements.razor`)**: Live-updating activity table highlighting the latest stock transaction types, reference documents, and amounts (automatically polling every 10 seconds).

---

## 2. Comprehensive Reports Portal (`WMS.Presentation/Components/Pages/Reports/`)

All reports include structured data tables, date range inputs, target selectors, and support inline CSV downloading (via a clean, lightweight JS download helper in `App.razor`):
- **Stock Snapshot (`StockSnapshot.razor`)**: Full status of active inventory quantities, reserved levels, and available counts grouped by categories and warehouses.
- **Movement History (`MovementHistory.razor`)**: Complete transactional logs of stock changes, filterable by product, location, type (Receipt, Issue, Transfer, Adjustment, Return), and dates.
- **Inbound Receipts (`InboundReport.razor`)**: Details receipt dispatches, suppliers, reference POs, locations, and batch number/expiry dates.
- **Outbound Issues (`OutboundReport.razor`)**: Summarizes customer dispatch orders, issues, dates, and locations.
- **Adjustments Ledger (`AdjustmentReport.razor`)**: Displays historical stock adjustments, discrepancy quantities (difference between actual and system levels), reason codes, and managers who created/approved them.
- **System Audit Trails (`AuditLog.razor`)**: Clean audit list of table modifications. Clicking a row expands a **Field-Level Diff Viewer** comparing the exact "Before" and "After" values key-by-key, parsed directly from the database JSON snapshot logs.

---

## 3. User & Settings portals (`WMS.Presentation/Components/Pages/Settings/` & `UserManagement/`)

- **IIdentityService (`WMS.Application` & `WMS.Infrastructure`)**: Mapped user management actions (creating, updating, resetting passwords, and role querying) to the Identity backend, keeping application handlers decoupled from concrete frameworks.
- **User Accounts Ledger (`UserList.razor`)**: Overview of user operators, active statuses, roles, and registration dates.
- **Account Form (`UserForm.razor`)**: Administrative screen to create new operators, assign multiple roles (`Admin`, `WarehouseManager`, `Receiver`, etc.), deactivate accounts, and override passwords. Protected by role-based authorization (`[Authorize(Roles = "Admin")]`).
- **App Settings (`AppSettings.razor`)**: Grid mapping the `AppSettings` table to configure company names, default currencies, addresses, and low stock warning triggers in real-time.
- **Change Password (`ChangePassword.razor`)**: Self-service security credentials reset page.

---

## Verification Guidelines

### 1. Build Verification
Confirm clean compile:
```powershell
dotnet build WMS.sln
```

### 2. Manual Testing Scenarios
1. **Interactive Dashboard**: Load `http://localhost:5000/` and verify that the KPI metrics load, the charts render properly in dark-mode aesthetics, and the activity feed refreshes periodically.
2. **CSV Exports**: Navigate to any page under the **Reports Portal**, apply filters, click **CSV**, and confirm that a file downloads containing the matching records.
3. **Audit Log Diffs**: Go to **System Audit Trails**, edit a product description or adjust inventory, and click **Show Changes** on the new log entry to verify the red/green property comparison table.
4. **User Permissions**: Create a new account with a Picker role, log in as that user, and verify they cannot access `/users` or `/settings` pages.
