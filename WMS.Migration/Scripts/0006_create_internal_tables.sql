-- 0006_create_internal_tables.sql

CREATE TABLE IF NOT EXISTS "TransferOrders" (
    "Id" UUID PRIMARY KEY,
    "TONumber" VARCHAR(50) NOT NULL UNIQUE,
    "FromWarehouseId" UUID NOT NULL REFERENCES "Warehouses"("Id") ON DELETE RESTRICT,
    "ToWarehouseId" UUID NOT NULL REFERENCES "Warehouses"("Id") ON DELETE RESTRICT,
    "Status" VARCHAR(50) NOT NULL, -- e.g., Draft, Approved, InTransit, Completed, Cancelled
    "RequestedBy" UUID NOT NULL,
    "ApprovedBy" UUID NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "TransferOrderItems" (
    "Id" UUID PRIMARY KEY,
    "TransferOrderId" UUID NOT NULL REFERENCES "TransferOrders"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "FromLocationId" UUID NOT NULL REFERENCES "Locations"("Id") ON DELETE RESTRICT,
    "ToLocationId" UUID NOT NULL REFERENCES "Locations"("Id") ON DELETE RESTRICT,
    "Qty" DECIMAL(18,4) NOT NULL,
    "Status" VARCHAR(50) NOT NULL -- e.g., Pending, Shipped, Received
);

CREATE TABLE IF NOT EXISTS "Returns" (
    "Id" UUID PRIMARY KEY,
    "ReturnNumber" VARCHAR(50) NOT NULL UNIQUE,
    "ReturnType" VARCHAR(50) NOT NULL, -- Customer or Supplier
    "ReferenceNo" VARCHAR(100) NOT NULL, -- original GR or GI reference
    "Status" VARCHAR(50) NOT NULL, -- Pending, Processing, Completed, Cancelled
    "Note" TEXT NULL,
    "CreatedBy" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS "ReturnItems" (
    "Id" UUID PRIMARY KEY,
    "ReturnId" UUID NOT NULL REFERENCES "Returns"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "LocationId" UUID NOT NULL REFERENCES "Locations"("Id") ON DELETE RESTRICT,
    "Quantity" DECIMAL(18,4) NOT NULL,
    "InspectionStatus" VARCHAR(50) NOT NULL, -- Pending, Accepted, Rejected
    "Note" TEXT NULL
);

CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" UUID PRIMARY KEY,
    "EntityName" VARCHAR(100) NOT NULL,
    "EntityId" VARCHAR(100) NOT NULL,
    "Action" VARCHAR(50) NOT NULL, -- Create, Update, Delete
    "OldValues" TEXT NULL, -- JSON string
    "NewValues" TEXT NULL, -- JSON string
    "UserId" UUID NULL,
    "Timestamp" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "AppSettings" (
    "Id" UUID PRIMARY KEY,
    "Key" VARCHAR(100) NOT NULL UNIQUE,
    "Value" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Group" VARCHAR(100) NOT NULL
);
