-- 0002_sample_suppliers_customers.sql

INSERT INTO "Suppliers" ("Id", "Code", "Name", "ContactPerson", "Phone", "Email", "Address", "IsActive", "CreatedAt") VALUES
('11111111-3333-3333-3333-333333333311', 'SUP-001', 'Global Tech Supplies', 'John Doe', '+155512345', 'info@globaltech.com', '10 Tech Way, San Jose CA', TRUE, CURRENT_TIMESTAMP),
('22222222-3333-3333-3333-333333333322', 'SUP-002', 'Acme Manufacturing', 'Jane Smith', '+155598765', 'orders@acmeparts.com', '456 Factory Rd, Chicago IL', TRUE, CURRENT_TIMESTAMP)
ON CONFLICT ("Code") DO NOTHING;

INSERT INTO "Customers" ("Id", "Code", "Name", "ContactPerson", "Phone", "Email", "Address", "IsActive", "CreatedAt") VALUES
('11111111-4444-4444-4444-444444444411', 'CUST-001', 'Apex Retailers Inc', 'Alice Cooper', '+155554321', 'purchasing@apexretail.com', '789 Market St, New York NY', TRUE, CURRENT_TIMESTAMP),
('22222222-4444-4444-4444-444444444422', 'CUST-002', 'Zenith Logistics & Distribution', 'Bob Marley', '+155587654', 'inbound@zenithlog.com', '321 Transit Way, Dallas TX', TRUE, CURRENT_TIMESTAMP)
ON CONFLICT ("Code") DO NOTHING;
