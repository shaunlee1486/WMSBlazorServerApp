-- 0001_sample_warehouses.sql

INSERT INTO "Warehouses" ("Id", "Code", "Name", "Address", "IsActive", "CreatedAt") VALUES
('11111111-1111-1111-1111-111111111111', 'WH-001', 'Main Distribution Center', '100 Industrial Pkwy, Logistics City', TRUE, CURRENT_TIMESTAMP),
('22222222-2222-2222-2222-222222222222', 'WH-002', 'East Coast Satellite', '500 Commerce Way, Boston MA', TRUE, CURRENT_TIMESTAMP)
ON CONFLICT ("Code") DO NOTHING;

INSERT INTO "Zones" ("Id", "WarehouseId", "Code", "Name", "ZoneType", "IsActive", "CreatedAt") VALUES
('11111111-2222-3333-4444-555555555555', '11111111-1111-1111-1111-111111111111', 'Z-REC', 'Receiving Dock', 'Receiving', TRUE, CURRENT_TIMESTAMP),
('22222222-3333-4444-5555-666666666666', '11111111-1111-1111-1111-111111111111', 'Z-STO', 'Bulk Storage Zone', 'Storage', TRUE, CURRENT_TIMESTAMP),
('33333333-4444-5555-6666-777777777777', '22222222-2222-2222-2222-222222222222', 'Z-STO-E', 'Satellite Storage', 'Storage', TRUE, CURRENT_TIMESTAMP)
ON CONFLICT ("WarehouseId", "Code") DO NOTHING;

INSERT INTO "Locations" ("Id", "ZoneId", "Aisle", "Bay", "Level", "Position", "Barcode", "MaxCapacity", "IsActive", "CreatedAt") VALUES
('11111111-aaaa-bbbb-cccc-dddddddddddd', '22222222-3333-4444-5555-666666666666', 'A', '1', '1', '1', 'LOC-A111', 1000, TRUE, CURRENT_TIMESTAMP),
('22222222-aaaa-bbbb-cccc-dddddddddddd', '22222222-3333-4444-5555-666666666666', 'A', '1', '1', '2', 'LOC-A112', 1000, TRUE, CURRENT_TIMESTAMP),
('33333333-aaaa-bbbb-cccc-dddddddddddd', '33333333-4444-5555-6666-777777777777', 'B', '1', '1', '1', 'LOC-B111', 500, TRUE, CURRENT_TIMESTAMP)
ON CONFLICT ("Barcode") DO NOTHING;
