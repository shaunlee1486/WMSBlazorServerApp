-- 0004_seed_app_settings.sql

INSERT INTO "AppSettings" ("Id", "Key", "Value", "Description", "Group") VALUES
('e081db8b-3e9a-4c22-9f7b-99f6ec36e6ba', 'CompanyName', 'WMS Enterprise Solutions', 'The name of the company operating the warehouse', 'General'),
('5c73d9a8-e4b6-4b68-8095-22a03cf1e9c2', 'CompanyAddress', '123 Logistics Blvd, Warehouse City', 'The physical address of the company office', 'General'),
('8fb6299b-4bfb-48ef-93a0-fcd84ff100d8', 'Currency', 'USD', 'Default currency used for financial reporting', 'General'),
('c0f0d2c3-9bfa-48ef-93a0-fcd84ff100d8', 'LowStockThreshold', '10', 'Default global threshold below which stock levels trigger a low-stock alert', 'Inventory')
ON CONFLICT ("Key") DO UPDATE SET
"Value" = EXCLUDED."Value",
"Description" = EXCLUDED."Description",
"Group" = EXCLUDED."Group";
