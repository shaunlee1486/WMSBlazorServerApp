-- 0004_create_inbound_tables.sql

CREATE TABLE IF NOT EXISTS "PurchaseOrders" (
    "Id" UUID PRIMARY KEY,
    "PONumber" VARCHAR(50) NOT NULL UNIQUE,
    "SupplierId" UUID NOT NULL REFERENCES "Suppliers"("Id") ON DELETE RESTRICT,
    "Status" VARCHAR(50) NOT NULL, -- e.g., Draft, Confirmed, PartialReceived, FullyReceived, Cancelled
    "OrderDate" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExpectedDate" TIMESTAMPTZ NULL,
    "Note" TEXT NULL,
    "CreatedBy" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "PurchaseOrderItems" (
    "Id" UUID PRIMARY KEY,
    "PurchaseOrderId" UUID NOT NULL REFERENCES "PurchaseOrders"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "OrderedQty" DECIMAL(18,4) NOT NULL,
    "ReceivedQty" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "UnitPrice" DECIMAL(18,4) NOT NULL
);

CREATE TABLE IF NOT EXISTS "GoodsReceipts" (
    "Id" UUID PRIMARY KEY,
    "GRNumber" VARCHAR(50) NOT NULL UNIQUE,
    "PurchaseOrderId" UUID NOT NULL REFERENCES "PurchaseOrders"("Id") ON DELETE RESTRICT,
    "ReceivedDate" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ReceivedBy" UUID NOT NULL,
    "Status" VARCHAR(50) NOT NULL, -- e.g., Draft, Completed, Cancelled
    "Note" TEXT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "GoodsReceiptItems" (
    "Id" UUID PRIMARY KEY,
    "GoodsReceiptId" UUID NOT NULL REFERENCES "GoodsReceipts"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "LocationId" UUID NOT NULL REFERENCES "Locations"("Id") ON DELETE RESTRICT,
    "ReceivedQty" DECIMAL(18,4) NOT NULL,
    "BatchNo" VARCHAR(100) NULL,
    "ExpiryDate" TIMESTAMPTZ NULL
);
