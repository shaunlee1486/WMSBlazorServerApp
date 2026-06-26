-- 0003_sample_categories_products.sql

INSERT INTO "Categories" ("Id", "Code", "Name", "ParentCategoryId", "CreatedAt") VALUES
('11111111-5555-5555-5555-555555555511', 'CAT-001', 'Electronics', NULL, CURRENT_TIMESTAMP),
('22222222-5555-5555-5555-555555555522', 'CAT-002', 'Hardware', NULL, CURRENT_TIMESTAMP)
ON CONFLICT ("Code") DO NOTHING;

INSERT INTO "Products" (
    "Id", "Code", "Name", "CategoryId", "UnitId", "SupplierId", 
    "Barcode", "Description", "ImagePath", "MinStock", "MaxStock", "ReorderPoint", "IsActive", "CreatedAt"
) VALUES (
    '11111111-6666-6666-6666-666666666611', 
    'PROD-001', 
    'Smart Hub Pro v2', 
    '11111111-5555-5555-5555-555555555511', -- Electronics
    '2b1db1eb-47bc-4995-8b35-46ba06f36402', -- pcs
    '11111111-3333-3333-3333-333333333311', -- Global Tech Supplies
    '9780201379624',
    'Advanced home automation smart gateway hub',
    NULL,
    10, 100, 15, TRUE, CURRENT_TIMESTAMP
), (
    '22222222-6666-6666-6666-666666666622', 
    'PROD-002', 
    'Heavy Duty Steel Bracket L-Type', 
    '22222222-5555-5555-5555-555555555522', -- Hardware
    '2b1db1eb-47bc-4995-8b35-46ba06f36402', -- pcs
    '22222222-3333-3333-3333-333333333322', -- Acme Manufacturing
    '9780201379625',
    '10-inch heavy duty reinforcement steel L-bracket',
    NULL,
    50, 500, 100, TRUE, CURRENT_TIMESTAMP
)
ON CONFLICT ("Code") DO NOTHING;
