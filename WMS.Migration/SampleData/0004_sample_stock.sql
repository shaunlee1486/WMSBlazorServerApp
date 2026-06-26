-- 0004_sample_stock.sql

INSERT INTO "Stock" ("Id", "ProductId", "LocationId", "Quantity", "ReservedQuantity", "LastUpdatedAt") VALUES
('11111111-7777-7777-7777-777777777711', '11111111-6666-6666-6666-666666666611', '11111111-aaaa-bbbb-cccc-dddddddddddd', 50, 5, CURRENT_TIMESTAMP),
('22222222-7777-7777-7777-777777777722', '11111111-6666-6666-6666-666666666611', '22222222-aaaa-bbbb-cccc-dddddddddddd', 20, 0, CURRENT_TIMESTAMP),
('33333333-7777-7777-7777-777777777733', '22222222-6666-6666-6666-666666666622', '33333333-aaaa-bbbb-cccc-dddddddddddd', 120, 15, CURRENT_TIMESTAMP)
ON CONFLICT ("ProductId", "LocationId") DO UPDATE SET
"Quantity" = EXCLUDED."Quantity",
"ReservedQuantity" = EXCLUDED."ReservedQuantity",
"LastUpdatedAt" = EXCLUDED."LastUpdatedAt";
