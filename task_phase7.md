# Phase 7 Tasks

- [x] Create domain entity `AppSetting.cs`
- [x] Map `AppSetting` in `InternalConfigurations.cs` and add DbSet to `AppDbContext.cs`
- [x] Implement Dashboard Queries (`GetDashboardStatsQuery`, `GetStockValueByCategoryQuery`, `GetInboundOutboundVolumeQuery`, `GetTopProductsQuery`)
- [x] Implement Report Queries (`GetStockSnapshotQuery`, `GetMovementHistoryReportQuery`, `GetInboundReportQuery`, `GetOutboundReportQuery`, `GetAdjustmentReportQuery`, `GetAuditLogsQuery`)
- [x] Implement User Management CQRS (`GetUsersQuery`, `GetUserByIdQuery`, `CreateUserCommand`, `UpdateUserCommand`, `ResetUserPasswordCommand`)
- [x] Update `_Imports.razor` with namespaces
- [x] Update `NavMenu.razor` with navigation links for Reports, Users, and Settings
- [x] Create Dashboard components (`KpiCard.razor`, `StockValueChart.razor`, `InboundOutboundChart.razor`, `TopProductsChart.razor`, `RecentMovements.razor`)
- [x] Update `Home.razor` to load dashboard components
- [x] Create Report pages with CSV download (`StockSnapshot.razor`, `MovementHistory.razor`, `InboundReport.razor`, `OutboundReport.razor`, `AdjustmentReport.razor`, `AuditLog.razor`)
- [x] Create User Management pages (`UserList.razor`, `UserForm.razor`)
- [x] Create Settings pages (`AppSettings.razor`, `ChangePassword.razor`)
- [x] Verify build compilation
