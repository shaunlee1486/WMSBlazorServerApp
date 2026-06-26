-- 0003_create_inventory_tables.sql

CREATE TABLE IF NOT EXISTS "Stock" (
    "Id" UUID PRIMARY KEY,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE CASCADE,
    "LocationId" UUID NOT NULL REFERENCES "Locations"("Id") ON DELETE CASCADE,
    "Quantity" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "ReservedQuantity" DECIMAL(18,4) NOT NULL DEFAULT 0,
    "LastUpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_stock_product_location UNIQUE ("ProductId", "LocationId")
);

CREATE TABLE IF NOT EXISTS "StockMovements" (
    "Id" UUID PRIMARY KEY,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE CASCADE,
    "FromLocationId" UUID NULL REFERENCES "Locations"("Id") ON DELETE SET NULL,
    "ToLocationId" UUID NULL REFERENCES "Locations"("Id") ON DELETE SET NULL,
    "Quantity" DECIMAL(18,4) NOT NULL,
    "MovementType" VARCHAR(50) NOT NULL, -- e.g., Receipt, Issue, Transfer, Adjustment, Return
    "ReferenceNo" VARCHAR(100) NULL,
    "Note" TEXT NULL,
    "CreatedBy" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "StockAdjustments" (
    "Id" UUID PRIMARY KEY,
    "AdjNumber" VARCHAR(50) NOT NULL UNIQUE,
    "WarehouseId" UUID NOT NULL REFERENCES "Warehouses"("Id") ON DELETE CASCADE,
    "AdjustmentDate" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Reason" TEXT NULL,
    "ApprovedBy" UUID NULL,
    "Status" VARCHAR(50) NOT NULL, -- e.g., Draft, PendingApproval, Approved, Rejected
    "CreatedBy" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "StockAdjustmentItems" (
    "Id" UUID PRIMARY KEY,
    "StockAdjustmentId" UUID NOT NULL REFERENCES "StockAdjustments"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE CASCADE,
    "LocationId" UUID NOT NULL REFERENCES "Locations"("Id") ON DELETE CASCADE,
    "SystemQty" DECIMAL(18,4) NOT NULL,
    "ActualQty" DECIMAL(18,4) NOT NULL,
    "Difference" DECIMAL(18,4) NOT NULL
);
