-- 0001_seed_roles.sql

INSERT INTO "Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp", "Description") VALUES
('d011f017-7422-4809-9f7b-99f6ec36e6ba', 'Admin', 'ADMIN', 'd011f017-7422-4809-9f7b-99f6ec36e6ba', 'Full system access, user management, settings'),
('b7f7c469-87a3-4886-9051-419b4a43b246', 'WarehouseManager', 'WAREHOUSEMANAGER', 'b7f7c469-87a3-4886-9051-419b4a43b246', 'All warehouse ops, approve adjustments/transfers, reports'),
('1f344b58-e3db-4e1e-aa67-73d7211153fb', 'Receiver', 'RECEIVER', '1f344b58-e3db-4e1e-aa67-73d7211153fb', 'Create/complete GoodsReceipts, view POs'),
('e1329a28-6625-4127-99e7-402a11444154', 'Picker', 'PICKER', 'e1329a28-6625-4127-99e7-402a11444154', 'View/work PickLists, create GoodsIssues'),
('79e13d9a-cc2e-4b68-8095-22a03cf1e9c2', 'StockController', 'STOCKCONTROLLER', '79e13d9a-cc2e-4b68-8095-22a03cf1e9c2', 'Stock adjustments, cycle counts, stock reports'),
('6e6de3bb-7d88-46a4-bb9e-4ebfa24a52ff', 'Viewer', 'VIEWER', '6e6de3bb-7d88-46a4-bb9e-4ebfa24a52ff', 'Read-only dashboard & reports')
ON CONFLICT ("Id") DO UPDATE SET
"Name" = EXCLUDED."Name",
"NormalizedName" = EXCLUDED."NormalizedName",
"Description" = EXCLUDED."Description";
