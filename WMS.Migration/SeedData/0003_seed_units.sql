-- 0003_seed_units.sql

INSERT INTO "Units" ("Id", "Code", "Name", "CreatedAt") VALUES
('2b1db1eb-47bc-4995-8b35-46ba06f36402', 'pcs', 'Pieces', CURRENT_TIMESTAMP),
('8d8f3ec1-19d2-43bb-a53d-d64ab41cc9a5', 'box', 'Box', CURRENT_TIMESTAMP),
('5c345330-811c-4b68-8095-a4f6be568019', 'carton', 'Carton', CURRENT_TIMESTAMP),
('9a826eb5-8e81-42ab-bb93-fa157be50965', 'pallet', 'Pallet', CURRENT_TIMESTAMP),
('68a2bf6e-3ff1-455b-a621-ee8de24a5f44', 'kg', 'Kilograms', CURRENT_TIMESTAMP),
('ef436bd3-a55e-436d-bfa2-3c812d1b1103', 'g', 'Grams', CURRENT_TIMESTAMP),
('f2c25674-c361-46ab-bb1e-128da24a5e22', 'l', 'Liters', CURRENT_TIMESTAMP),
('b8c4d2d4-a82f-4886-9051-fb1ca43b27b2', 'ml', 'Milliliters', CURRENT_TIMESTAMP)
ON CONFLICT ("Id") DO UPDATE SET
"Code" = EXCLUDED."Code",
"Name" = EXCLUDED."Name";
